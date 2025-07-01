namespace TodoWebApp
{
    public static class Constants
    {
        const string defaultComp = "TodoWebApp";
        public const string AppBuild = "BETA";
        public static string GetCurrentNamespace() => System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace ?? defaultComp;
        public static string GetCurrentFullName() => System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName ?? defaultComp;
        public static string GetCurrentAssemblyName() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? defaultComp;
        public static Version GetCurrentAssemblyVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(); // Returns the AssemblyVersion, not the FileVersion.

    }
}
