namespace Social_Anxiety
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rota do Registro individual (detalhe de um check-in): record?index=N
            Routing.RegisterRoute("record", typeof(RecordPage));
        }
    }
}
        