using System.Linq;
using System.Windows;

namespace SokolovExamen
{
    public partial class MainWindow : Window
    {
        private readonly BDModel _db;

        public MainWindow()
        {
            InitializeComponent();
            _db = new BDModel();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = tbLogin.Text.Trim();
            var password = pbPassword.Password;

            var user = _db.User.FirstOrDefault(u =>
                u.Login == login &&
                u.Password == password);

            if (user == null)
            {
                ShowLoginError();
                return;
            }

            OpenProducts(user);
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            OpenProducts(null);
        }

        private void OpenProducts(User user)
        {
            var window = new ProductsList(user);
            window.Show();
            Close();
        }

        private void ShowLoginError()
        {
            MessageBox.Show(
                "Логин или пароль указаны неверно",
                "Ошибка авторизации",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}