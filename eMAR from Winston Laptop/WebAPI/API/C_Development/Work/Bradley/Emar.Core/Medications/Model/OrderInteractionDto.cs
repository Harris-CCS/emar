using System;
using System.Collections.Generic;

namespace Emar.Core.Medications.Model
{
    public class OrderInteractionDto
    {
        public long Id { get; set; }
        public long MedicationInteractionId { get; set; }
        public byte DrugNum { get; set; }
        public long? PatientOrderId { get; set; }
        public long? PatientCartOrderId { get; set; }
        public long? PatientHomeMedicationId { get; set; }

        public DrugInteractionViewDto DrugInteraction { get; set; }
    }
    public class OrderInteractionDtoComparer : IEqualityComparer<OrderInteractionDto>
    {
        public bool Equals(OrderInteractionDto x, OrderInteractionDto y)
        {
            return
                x.DrugInteraction?.InteractionOrderName == y.DrugInteraction?.InteractionOrderName &&
                x.DrugInteraction?.Severity == y.DrugInteraction?.Severity;
        }

        public int GetHashCode(OrderInteractionDto obj)
        {
            //Prior logic.
            //return obj.DrugInteraction == null ? 0.GetHashCode() :
            //    obj.DrugInteraction.InteractionOrderName.GetHashCode() ^
            //    obj.DrugInteraction.Severity.GetHashCode();

            //We're seeing a situation where a patient has a null PatientOrderId for one of
            //the two rows for an OrderInteraction.  This causes the view to not be able to
            //get the InteractionOrderName which causes the "get orders" call to hit a 500
            //error when calling GetHashCode on it.
            //I'm not sure what a "Hash Code" is here, or why we need it.
            //But I think that handling the null values and returning the hascode for 0
            //here (as we do when the interaction is null), will work.
            //I'm going to manually setup the data on a patient on 57c to be this exact
            //scenario, then I'm going to test this with Postman/Visual Studio.
            //Winston Murdock, 05/02/2022.

            int defaultHashCode = 0.GetHashCode();
            int interactionOrderNameHashCode;
            int severityHashCode;

            try
            {
                if (obj.DrugInteraction == null)
                {
                    //DrugInteraction is null.
                    return defaultHashCode;
                }
                else
                {
                    //DrugInteraction is not null.
                    if (!string.IsNullOrEmpty(obj.DrugInteraction.InteractionOrderName))
                    {
                        //InteractionOrderName is not null.  Use its hash code.
                        interactionOrderNameHashCode = obj.DrugInteraction.InteractionOrderName.GetHashCode();
                    }
                    else
                    {
                        //InteractionOrderName is null.  Use the default hash code.
                        interactionOrderNameHashCode = defaultHashCode;
                    }

                    if (!string.IsNullOrEmpty(obj.DrugInteraction.Severity))
                    {
                        //Severity is not null.  Use its hash code.
                        severityHashCode = obj.DrugInteraction.Severity.GetHashCode();
                    }
                    else
                    {
                        //Severity is null.  Use the default hash code.
                        severityHashCode = defaultHashCode;
                    }

                    //If either interaction order name hash code or severity hash code are the default, then we'll just return the default.
                    if ((interactionOrderNameHashCode == defaultHashCode) || (severityHashCode == defaultHashCode))
                    {
                        //One or both of the has codes are the default.
                        //Just return the default.
                        return defaultHashCode;
                    }
                    else
                    {
                        //Neither of the has codes is the default.
                        //Return them both.
                        return  obj.DrugInteraction.InteractionOrderName.GetHashCode() ^ obj.DrugInteraction.Severity.GetHashCode();
                    } //end if
                } //end if
            }
            catch (Exception ex)
            {
                //Handle the error ourselves rather than having this exception be handled by .NET and generate a 500 error.
                return defaultHashCode;
            }//end try/catch
        } //end GetHashCode()
    }
}