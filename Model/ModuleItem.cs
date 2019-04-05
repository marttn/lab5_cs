using System.Diagnostics;

namespace lab5.Model
{
    class ModuleItem
    {
        private readonly ProcessModule module;
        public string ModuleName => module.ModuleName;
        public string ModulePath => module.FileName;

        public ModuleItem(ProcessModule module)
        {
            this.module = module;
        }
    }
}
