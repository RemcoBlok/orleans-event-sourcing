using System.Reflection;

namespace Banking.Persistence.AzureStorage.Serialization
{
    public static class TypeExtensions
    {
        public static string GetSimpleAssemblyQualifiedName(this Type type)
        {
            return Assembly.CreateQualifiedName(AssemblyNameCache.GetName(type.Assembly).Name, type.FullName);
        }
    }
}
