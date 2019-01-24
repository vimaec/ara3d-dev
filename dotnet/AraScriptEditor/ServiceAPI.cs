using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Windows.Forms;


// https://docs.microsoft.com/en-us/dotnet/framework/wcf/migrating-from-net-remoting-to-wcf
// https://dopeydev.com/wcf-interprocess-communication/
namespace Ara3D
{
    /// <summary>
    /// The editor will call these methods depending on what the user does. 
    /// Everything else is managed by the editor itself. 
    /// </summary>
    public interface IEditorClientCallback
    {
        [OperationContract]
        string[] CompileAndRun(string text);

        [OperationContract]
        string NewSnippet();
    }

    /// <summary>
    /// The editor service just needs to be connected to: the client will provide an IEditorCallbackService
    /// The client knows how to compile, the editor just knows how to edit. The Client also knows how to create new snippets
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IEditorClientCallback))]
    public interface IEditorService
    {
        [OperationContract]
        void Connect();
    }

    /// <summary>
    /// The concrete implementation of the EditorService
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class EditorService : IEditorService
    {
        /// <summary>
        /// Nothing happens during the connect except recieving the callback contract from the client.
        /// </summary>
        public void Connect()
        {
            Client = OperationContext.Current.GetCallbackChannel<IEditorClientCallback>();
        }

        public static IEditorClientCallback Client { get; set; }
    }

    /// <summary>
    /// Launches the service executable. You link to this exe like a class library, and then call this function. 
    /// </summary>
    public static class ServiceLauncher
    {
        public static void LaunchProcess()
        {
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = GetExePath();
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForInputIdle();
            }
        }

        public static void Connect(IEditorClientCallback callback)
        {
            // Connect to the service using WCF
            var context = new InstanceContext(callback);
            var pipeFactory = new DuplexChannelFactory<IEditorService>(
                context,
                new NetNamedPipeBinding(),
                new EndpointAddress(ServiceConfig.EndPointURI));
            var service = pipeFactory.CreateChannel();
            service.Connect();
        }

        public static string GetExePath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }
    }

    /// <summary>
    /// Contains the configuration information for the service, and opens a WCF communication endpoint. 
    /// </summary>
    public static class ServiceConfig
    {
        public static string URI = "net.pipe://localhost";
        public static string EndPoint = "ScriptEditor";
        public static string EndPointURI = $"{URI}/{EndPoint}";

        /// <summary>
        /// Starts the service and opens an end-point.
        /// </summary>
        public static void Start()
        {
            var host = new ServiceHost(typeof(EditorService), new Uri(URI));
            host.AddServiceEndpoint(typeof(IEditorService), new NetNamedPipeBinding(), EndPoint);
            host.Open();
        }
    }

    /// <summary>
    /// The entry point for the program.
    /// </summary>
    public static class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDPIAware();

        [STAThread]
        public static void Main()
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/winforms/automatic-scaling-in-windows-forms
            // https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-improve-performance-by-avoiding-automatic-scaling
            SetProcessDPIAware();
            ServiceConfig.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ScriptEditorForm());
        }
    }
}
