using System.Windows;
using System.Windows.Controls;

namespace MakeGrid3D.Controls
{
    public partial class BoundaryConditionControl : UserControl
    {
        public BoundaryConditionControl()
        {
            InitializeComponent();
        }

        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Ug
        {
            get => (string)GetValue(UgProperty);
            set => SetValue(UgProperty, value);
        }

        public string Theta
        {
            get => (string)GetValue(ThetaProperty);
            set => SetValue(ThetaProperty, value);
        }

        public string UBeta
        {
            get => (string)GetValue(UBetaProperty);
            set => SetValue(UBetaProperty, value);
        }

        public string Beta
        {
            get => (string)GetValue(BetaProperty);
            set => SetValue(BetaProperty, value);
        }

        public int SelectedOption
        {
            get => (int)GetValue(SelectedOptionProperty);
            set => SetValue(SelectedOptionProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(BoundaryConditionControl));

        public static readonly DependencyProperty UgProperty =
            DependencyProperty.Register("Ug", typeof(string), typeof(BoundaryConditionControl));

        public static readonly DependencyProperty ThetaProperty =
            DependencyProperty.Register("Theta", typeof(string), typeof(BoundaryConditionControl));

        public static readonly DependencyProperty UBetaProperty =
            DependencyProperty.Register("UBeta", typeof(string), typeof(BoundaryConditionControl));

        public static readonly DependencyProperty BetaProperty =
            DependencyProperty.Register("Beta", typeof(string), typeof(BoundaryConditionControl));

        public static readonly DependencyProperty SelectedOptionProperty =
            DependencyProperty.Register("SelectedOption", typeof(int), typeof(BoundaryConditionControl),
                new PropertyMetadata(0));
    }
}