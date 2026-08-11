using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ProgressBarPage : UserControl
{
    public ProgressBarPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <ProgressBar Value="70" />
            <ProgressBar Value="100" Style="{StaticResource ProgressBarSuccess}" />
            <ProgressBar Value="55"  Style="{StaticResource ProgressBarInfo}" />
            <ProgressBar Value="40"  Style="{StaticResource ProgressBarWarning}" />
            <ProgressBar Value="22"  Style="{StaticResource ProgressBarDanger}" />
            """;

        Ex2.Xaml = """
            <ProgressBar Value="70" Style="{StaticResource ProgressBarPrimaryStripe}" />
            <ProgressBar Value="55" Style="{StaticResource ProgressBarSuccessStripe}" />
            <ProgressBar Value="45" Style="{StaticResource ProgressBarInfoStripe}" />
            <ProgressBar Value="35" Style="{StaticResource ProgressBarWarningStripe}" />
            <ProgressBar Value="25" Style="{StaticResource ProgressBarDangerStripe}" />
            """;

        Ex3.Xaml = """
            <ProgressBar Value="60" Style="{StaticResource ProgressBarFlat}" />

            <ProgressBar IsIndeterminate="True" />
            <ProgressBar IsIndeterminate="True"
                         Style="{StaticResource ProgressBarSuccess}" />
            """;
    }
}
