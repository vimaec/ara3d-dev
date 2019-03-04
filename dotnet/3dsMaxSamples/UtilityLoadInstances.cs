namespace Ara3D
{
    public class UtilityLoadInstances : IUtilityPlugin
    {
        public void Evaluate()
        {
            API.LoadG3DFiles(@"C:\dev\tmp\vim-export-demo-copy");
        }
    }
}