using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class ProgressRingPage : UserControl
{
    public ProgressRingPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <ui:ProgressRing Value="25" />
            <ui:ProgressRing Value="60"  Style="{StaticResource ProgressRingSuccess}" />
            <ui:ProgressRing Value="85"  Style="{StaticResource ProgressRingWarning}" />
            <ui:ProgressRing Value="100" Style="{StaticResource ProgressRingDanger}" />
            """;

        Ex2.Xaml = """
            <ui:ProgressRing IsIndeterminate="True" />

            <ui:ProgressRing IsIndeterminate="True"
                             RingSize="40"
                             Style="{StaticResource ProgressRingSuccess}" />
            """;
    }
}
