using Emar.Core.InboundData.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace Emar.Core.InboundData.Service.IbexSpecific
{
    public class IbexSqlMessageProcessorHostedService : BackgroundService
    {

        private readonly ILogger<IbexSqlMessageProcessorHostedService> _logger;
        private readonly SqlQueueNotificationChannel _channel;
        private readonly IServiceProvider _service;

        public IbexSqlMessageProcessorHostedService(ILogger<IbexSqlMessageProcessorHostedService> logger,
            SqlQueueNotificationChannel channel, IServiceProvider service)
        {
            _logger = logger;
            _channel = channel;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IbexSqlMessageProcessorHostedService Hosted Service running.");

            // We're using the channel to cue us to when there are records written to the queue.
            // However, when we are awakened by records in the channel, we're running the database queue until all records 
            // are processed.  When we get to that end-point, we are recording the top record in the database queue
            // at that point.  Any 

            // Pull the updated records out of the channel one at a time and process them
            long topRecordProcessed = 0;

            //List of error records.
            //If we hit five times not being able to process a record, we'll remove it from here,
            //will log the issue, and and will mark it as completed.
            //Also, wehn we successfully process a record, we'll attempt to remove a record
            //from here (in case it did error at first but then did process before the fifth time).
            Dictionary<string, int> errorCount = new Dictionary<string, int>();

            //This loop wauits until there is one or more records to process in the queue table.
            //Then it processes all records and keeps waiting until there is another record to process.
            await foreach (var newQueueRecord in _channel.ReadAllAsync(stoppingToken))
            {
                if (newQueueRecord.Id <= topRecordProcessed)
                {
                    _logger.LogInformation(
                        $"Pulled Record #{newQueueRecord.Id} off of the Channel (already processed)");
                    continue;
                }
                _logger.LogInformation($"Pulled Record #{newQueueRecord.Id} off of the Channel");

                using var scope = _service.CreateScope();

                var dataProcessor = scope.ServiceProvider.GetService<IIbexIdsProcessorService>();
                if (dataProcessor == null)
                    throw new NullReferenceException("Scoped Service IbexContext not available in the DI pipeline.");

                NextQueueRecordToProcessDto record = null;

                //****************************************************
                //Add a do while (true) loop around the while loop that gets the next record.
                //Add a try/catch around the while loop that gets the next record.
                //If we ever can't get the next record, continue to the next iteration of the do loop
                //(which will try to get the same record again).
                //Add a try/catch around the call to process the record.
                //If we failed processing it with the counter variable at five
                //then log that we failed processing it
                //and leave the record variable alone so that we update the complete_datetime
                //and move to the next record in the queue table.
                //In that case, there's some error we'll never be able to overcome.
                //A datatype difference between the ibex and emar databases, or something similar.
                //If we failed processing it with the counter variable less than five,
                //then log that we failed processing it and set the record vairable
                //to null so that we attempt to grab this same record and process it again.
                //If we fail processing it, set the 
                //Winston Murdock, 11/19/2021.  PC-26691
                //****************************************************

                //This do loop continues while true.
                //Thus, it never ends unelss we force a break.
                //That is exactly what we want.
                do
                {
                    //try to get the next record.
                    //The SP updates the inprocess time while it gets the record.
                    //So there is a chance that we could be the deadlock victim
                    //if one of the triggers on the ibex database were writing to the
                    //queue table at the exact same time we try to update the inprocess time.
                    //If we are the deadlock victim, then continue to the next time through
                    //the loop which will try to get this record again.
                    //This logic will never skip a record in the queue table.
                    try
                    {
                        //if we cannot get the next record, then break the loop.
                        //This call will set the complete timestamp on the last
                        //record that we processed and will return us a blank/null
                        //record, which the helper methods handles for us.
                        if (!dataProcessor.GetNextQueueRecordToProcess(ref record))
                        {
                            //We did not get a record (i.e. they weren't any to get).
                            //So break from the neverending do loop.
                            //Else, we would always be stuck in here.
                            break;
                        } //end if
                    }
                    catch (Exception ex)
                    {
                        //We could not pull this record.
                        //Most likely, we were unable to update the inprocess date time
                        //due to being the deadlock victim.
                        //We could log that, but I don't want to clog up the Event Viewer.

                        //Wait two seconds.
                        await Task.Delay(2000, stoppingToken);

                        //I'm not sure why I ever set this to null.
                        //This is likely the cause of our issues on Emerus Test.
                        //Here's what was happening.
                        //1) Pull in an row from the queue table and mark it as inprogress.
                        //2) Process that row.
                        //3) On the next iteration through the loop, attempt to mark that record as
                        //     complete and grab a new record (marking the new record as inprocess).
                        //4 Deadlock in the SP.
                        //5) Catch the exception.
                        //6) Set record to null (meaning we can't mark the last record we processed
                        //     as complete on the next time through the loop.
                        //7) Profit????
                        //
                        //Not setting record to null here, allows the next time through the loop
                        //to complete this record then grab the next one.
                        //
                        //I'm not sure if this is the cause of the delay in pulling
                        //in items from ibex.  But It feels reasonable to think it is.
                        //Winston Murdock, 01/24/2022.
                        //record = null;

                        //Then continue the do loop which will try to pull this same record again.
                        //The record variable will be the most recent row we have succesfully pulled from the DB.
                        //The next loop will set its complete time to now.
                        continue;
                    } //end try/catch (attempting to get the record)

                    try
                    {
                        //Process the record from the queue table.
                        dataProcessor.ProcessUpdatedRecord(record.RecordType, record.RecordExternalId);

                        //If we successfully processed this record,
                        //then remove it from the error list.
                        //If it errored before but worked now, it
                        //is in the error list.
                        //If this was the first time we came across this
                        //error, it will not be in the error list.
                        //In that case, we won't try ro remove it.
                        if (errorCount.ContainsKey(record.QueueRecordId))
                        {
                            errorCount.Remove(record.QueueRecordId);
                        } //end if

                        //Get the maximum id that has been processed.
                        topRecordProcessed = record.HighestQueueIdWhenQuerying;
                    }
                    catch (Exception ex)
                    {
                        //There was an error processing the record.

                        //If this record is not already in the error list, add it to the list.
                        if (!errorCount.ContainsKey(record.QueueRecordId))
                        {
                            errorCount.Add(record.QueueRecordId, 0);
                        } //end if


                        //Some error processing the record we pulled.
                        //Data type differences between ibex and emar or something systemic.
                        //Retry five times.
                        //If we don't get it working after five times,
                        //log to the Event Viewer and move to the next record.
                        int errors;
                        if ((errors = ++errorCount[record.QueueRecordId]) > 5)
                        {
                            //We've already tried five times.
                            //We cannot successfully process this record.
                            //Log that it's a record we cannot process.
                            using (EventLog eventLog = new EventLog("Application"))
                            {
                                string sException = "Record = " + record.QueueRecordId + "\'";
                                sException += ex.Message + "\n";
                                sException += "source = " + ex.Source + "\n";
                                sException += ex.StackTrace + "\n";

                                eventLog.Source = "PulseCheck EMAR API";
                                eventLog.WriteEntry(sException, EventLogEntryType.Error, 101, 1);
                            } //end using.

                            //Since we're continuing past this record, remove it from the error dictionary.
                            //It should be in the error list.  But lets do the if check, just in case.
                            if (errorCount.ContainsKey(record.QueueRecordId))
                            {
                                errorCount.Remove(record.QueueRecordId);
                            } //end if

                            //Leave record as it is, so that the next iteration marks
                            //it as completed and moves on to the next record.
                        } //end if (counter >= 5?)
                        //else
                        //{
                        //    //We could log that this failed.
                        //    //But I don't want to log the five attempts and then log that we're skipping this one.
                        //    //We'll only log this record in the if statement above when we've tried five times already.

                        //    //Need this to not be null.
                        //    //We want the SP to think we processed this,
                        //    //mark it as completed on the next iteration,
                        //    //and move to the next record.
                        //    //record = null;
                        //} //end if (counter >= 5?)
                    } //end try/catch (attempting to process the record)
                } while (true);
            } //await (foreach)
        }

        /// <summary>
        /// override of the base class so that we can log an informational message
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EmarIdsDataTransferHostedService is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
