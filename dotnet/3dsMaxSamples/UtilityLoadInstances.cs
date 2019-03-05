namespace Ara3D
{
    public class UtilityLoadInstances : IUtilityPlugin
    {
        public void Evaluate()
        {
            API.LoadBFastScene(@"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-04_23-12-13-main\output.vim");
        }
    }
}