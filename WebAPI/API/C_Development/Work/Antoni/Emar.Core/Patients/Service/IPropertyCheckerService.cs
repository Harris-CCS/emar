#if EXPANDO
namespace Emar.Core.Patients.Service
{
    public interface IPropertyCheckerService
    {
        bool TypeHasProperties<T>(string fields);
    }
}
#endif
