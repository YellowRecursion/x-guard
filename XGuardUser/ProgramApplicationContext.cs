namespace XGuardUser
{
    internal sealed class ProgramApplicationContext : ApplicationContext
    {
        public ProgramApplicationContext()
        {

        }

        public ProgramApplicationContext(Form? mainForm) : base (mainForm)
        {

        }

        protected override async void Dispose(bool disposing)
        {
            if (disposing)
            {
                await DisposeResourcesAsync();
            }
            base.Dispose(disposing);
        }

        private async Task DisposeResourcesAsync()
        {
            await XGuardMain.Instance.DisposeAsync();
        }
    }
}
