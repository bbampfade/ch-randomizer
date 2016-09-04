using System.Windows;
using System;
using CH2.Helpers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace CH2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AxWMPLib.AxWindowsMediaPlayer player = null;

        CHDB chdb;
        private RoundAnnotator ra;
        private Storyboard storyboardWave;
        private Storyboard storyboardRotation;
        private readonly string me;

        public MainWindow()
        {
            var wpfAssembly = (AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(item => item.EntryPoint != null)
                .Select(item =>
                    new { item, applicationType = item.GetType(item.GetName().Name + ".App", false) })
                .Where(a => a.applicationType != null && typeof(System.Windows.Application)
                    .IsAssignableFrom(a.applicationType))
                    .Select(a => a.item)).FirstOrDefault();

            me = Path.GetDirectoryName(wpfAssembly.Location);

            this.Loaded += MainWindow_Loaded;
            InitializeComponent();
            InitMediaPlayer();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loadCHDB();
        }

        private void loadCHDB()
        {

            chdb = new CHDB(me);

            DataContext = chdb;
            ra = new RoundAnnotator(chdb);
            ra.Show();
        }

        private void InitMediaPlayer()
        {
            player = (AxWMPLib.AxWindowsMediaPlayer)formsHost.Child;
            PlayerHelper.Player = player;
            PlayerHelper.SoftPath = me;
        }

        private void lbVideos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var listbox = sender as ListBox;
            listbox.ScrollIntoView(e.AddedItems[0]);
        }

        private void fileChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext != null)
            {
                var combo = sender as ComboBox;
                var item = combo.SelectedItem as ComboBoxItem;
                var selected = item.Content as string;
                if (CheckAccess()) // don't run this in the dispatcher
                {
                    Task.Run(() => chdb.LoadNewRoot(selected));
                }
            }
        }

        private void roundControlShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (ra == null) // why? i dunno
            {
                ra = new RoundAnnotator(chdb);
            }
            ra.Show();
        }

        private void setupTextAnimations(object sender, RoutedEventArgs e)
        {
            this.breakTextBlock.TextEffects = new TextEffectCollection();

            storyboardWave = new Storyboard();

            storyboardRotation = new Storyboard();

            storyboardRotation.RepeatBehavior = RepeatBehavior.Forever;
            storyboardRotation.AutoReverse = true;

            for (int i = 0; i < this.breakTextBlock.Text.Length; ++i)
            {
                this.AddTextEffectForCharacter(i);
                this.AddWaveAnimation(storyboardWave, i);
                this.AddRotationAnimation(storyboardRotation, i);
            }

            Timeline pause = this.FindResource("CharacterRotationPauseAnimation") as Timeline;

            storyboardRotation.Children.Add(pause);
        }

        private void AddRotationAnimation(Storyboard storyboardRotation, int i)
        {
            DoubleAnimation anim = this.FindResource("CharacterRotationAnimation") as DoubleAnimation;

            this.SetBeginTime(anim, i);

            string path = String.Format(
                "TextEffects[{0}].Transform.Children[1].Angle", i);

            PropertyPath propPath = new PropertyPath(path);
            Storyboard.SetTargetProperty(anim, propPath);
            storyboardRotation.Children.Add(anim);
        }

        private void SetBeginTime(DoubleAnimation anim, int i)
        {
            double totalMs = anim.Duration.TimeSpan.TotalMilliseconds;
            double offset = totalMs / 10;
            double resolvedOffset = offset * i;
            anim.BeginTime = TimeSpan.FromMilliseconds(resolvedOffset);
        }

        private void AddWaveAnimation(Storyboard storyboardWave, int i)
        {
            DoubleAnimation anim =
                this.FindResource("CharacterWaveAnimation")
                as DoubleAnimation;

            this.SetBeginTime(anim, i);

            string path = String.Format(
                "TextEffects[{0}].Transform.Children[0].Y",
                i);

            PropertyPath propPath = new PropertyPath(path);
            Storyboard.SetTargetProperty(anim, propPath);

            storyboardWave.Children.Add(anim);
        }

        private void AddTextEffectForCharacter(int i)
        {
            TextEffect effect = new TextEffect();
            effect.PositionStart = i;
            effect.PositionCount = 1;

            TransformGroup transGrp = new TransformGroup();
            transGrp.Children.Add(new TranslateTransform());
            transGrp.Children.Add(new RotateTransform());
            effect.Transform = transGrp;

            this.breakTextBlock.TextEffects.Add(effect);

        }

        private void startTextAnimations(object sender, DependencyPropertyChangedEventArgs e)
        {
            UIElement senderUI = sender as UIElement;
            if (senderUI.IsVisible)
            {
                if (storyboardWave != null)
                {
                    storyboardWave.Begin(this, true);
                    storyboardRotation.Begin(this, true);
                }
            }
            else
            {
                if (storyboardWave != null)
                {
                    storyboardRotation.Stop(this);
                    storyboardWave.Stop(this);
                }
            }            
        }
    }
}
