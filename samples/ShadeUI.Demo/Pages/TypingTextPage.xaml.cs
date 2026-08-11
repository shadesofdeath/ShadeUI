using System.Windows.Controls;

namespace ShadeUI.Demo.Pages;

public partial class TypingTextPage : UserControl
{
    public TypingTextPage()
    {
        InitializeComponent();

        Ex1.Xaml = """
            <ui:TypingText Loop="True"
                           TypeSpeed="65"
                           DeleteSpeed="35"
                           PauseDelay="1400"
                           TextStyle="Subtitle">
                <ui:TypingText.Words>
                    <sys:String>tema kütüphanesi</sys:String>
                    <sys:String>kontrol seti</sys:String>
                </ui:TypingText.Words>
            </ui:TypingText>
            """;

        Ex2.Xaml = """
            <ui:TypingText Text="dosya adı yazılıyor"
                           Duration="70"
                           Loop="True"
                           CursorStyle="Line" />

            <ui:TypingText Text="imleçsiz, tek sefer yazılır"
                           Delay="400"
                           ShowCursor="False" />
            """;
    }
}
