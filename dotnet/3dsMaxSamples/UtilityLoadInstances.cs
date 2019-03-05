namespace Ara3D
{
    public class UtilityLoadInstances : IUtilityPlugin
    {
        public void Evaluate()
        {
            API.LoadBFastScene(@"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-05_01-24-32-main\output.vim");
        }
    }
}