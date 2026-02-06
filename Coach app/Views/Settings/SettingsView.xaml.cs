using Coach_app.ViewModels.Settings;

namespace Coach_app.Views.Settings;

public partial class SettingsView : ContentPage
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        // C'est cette ligne magique qui relie le bouton au Command :
        BindingContext = vm;
    }
}