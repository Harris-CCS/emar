namespace PulseCheck.Archive.Data
{
    public interface IIbexArchiveRepository
    {
        void ArchiveOrdResults(int saveDaysBack, int batchCnt = 1000);
    }
}