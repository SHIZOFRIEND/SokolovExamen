using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SokolovExamen
{
    public partial class ProductsList : Window
    {
        private BDModel _db = new BDModel();
        private ObservableCollection<ProductView> _data = new ObservableCollection<ProductView>();
        private ICollectionView _collection;

        private User _currentUser;
        private AddTovar _editWindow;

        private string _search = "";

        private bool _guest;
        private bool _client;
        private bool _admin;
        private bool _manager;

        public ProductsList(User user)
        {
            InitializeComponent();

            _currentUser = user;

            InitRole();
            FillUserPanel();
            ApplyRights();

            _collection = CollectionViewSource.GetDefaultView(_data);
            _collection.Filter = FilterProducts;

            dgProducts.ItemsSource = _collection;

            LoadProducts();
        }

        private void InitRole()
        {
            _guest = _currentUser == null;

            if (_currentUser != null && _currentUser.Role != null)
            {
                string role = _currentUser.Role.RoleName;

                _client = role == "Авторизированный клиент";
                _admin = role == "Администратор";
                _manager = role == "Менеджер";
            }
        }

        private void FillUserPanel()
        {
            if (_currentUser == null)
            {
                tbUser.Text = "Гость";
                tbRole.Text = "Неавторизован";
                return;
            }

            tbUser.Text = _currentUser.Familia + " " + _currentUser.Imya + " " + _currentUser.Otchestvo;
            tbRole.Text = _currentUser.Role.RoleName;
        }

        private void ApplyRights()
        {
            bool canViewOrder = _admin || _manager;

            tbSearch.Visibility = (_admin || _manager)
                ? Visibility.Visible
                : Visibility.Collapsed;

            btnOrder.Visibility = canViewOrder
                ? Visibility.Visible
                : Visibility.Collapsed;

            btnAdd.Visibility = _admin
                ? Visibility.Visible
                : Visibility.Collapsed;

            btnDelete.Visibility = _admin
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void LoadProducts()
        {
            _data.Clear();

            foreach (var p in _db.Tovar.ToList())
            {
                int discount = p.Discount ?? 0;
                int stock = p.KolvoNaSklade ?? 0;
                decimal cost = p.Cost;

                decimal final = cost - (cost * discount / 100);

                Brush brush = Brushes.Transparent;

                if (stock <= 0)
                    brush = Brushes.LightGray;
                else if (discount > 15)
                    brush = (Brush)new BrushConverter().ConvertFrom("#008080");

                _data.Add(new ProductView
                {
                    Id = p.IDTovar,
                    TovarName = p.TovarName,
                    Description = p.Description,
                    CategoryName = p.TovarCategory != null ? p.TovarCategory.TovarCategoryName : "",
                    Manufacturer = p.Proizvoditeli != null ? p.Proizvoditeli.NameProizvoditelia : "",
                    Supplier = p.Postavshik != null ? p.Postavshik.PostavshikName : "",
                    Cost = cost,
                    FinalPrice = final,
                    Discount = discount,
                    Unit = p.EdIzmer != null ? p.EdIzmer.Edzm : "",
                    Stock = stock,
                    RowBrush = brush
                });
            }

            _collection.Refresh();
        }

        

        private bool FilterProducts(object obj)
        {
            var item = obj as ProductView;

            if (item == null)
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string s = _search;

            return (item.TovarName != null && item.TovarName.ToLower().Contains(s)) ||
                   (item.Description != null && item.Description.ToLower().Contains(s)) ||
                   (item.CategoryName != null && item.CategoryName.ToLower().Contains(s)) ||
                   (item.Manufacturer != null && item.Manufacturer.ToLower().Contains(s)) ||
                   (item.Supplier != null && item.Supplier.ToLower().Contains(s));
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _search = tbSearch.Text.Trim().ToLower();
            _collection.Refresh();
        }

 

        private void dgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ProductView selected = dgProducts.SelectedItem as ProductView;

            if (selected == null)
                return;

            if (!_admin)
            {
                MessageBox.Show("Только администратор может редактировать");
                return;
            }

            var entity = _db.Tovar.FirstOrDefault(x => x.IDTovar == selected.Id);

            if (entity == null)
                return;

            _editWindow = new AddTovar(entity);

            if (_editWindow.ShowDialog() == true)
            {
                _db.SaveChanges();
                LoadProducts();
            }
        }
 

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Tovar t = new Tovar();
            AddTovar w = new AddTovar(t);

            if (w.ShowDialog() == true)
            {
                _db.SaveChanges();
                LoadProducts();
            }
        }
 

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ProductView selected = dgProducts.SelectedItem as ProductView;

            if (selected == null)
                return;

            var entity = _db.Tovar.FirstOrDefault(x => x.IDTovar == selected.Id);

            if (entity == null)
                return;

            if (_db.Zakaz.Any(x => x.IDTovar == entity.IDTovar))
            {
                MessageBox.Show("Нельзя удалить товар (есть заказы)");
                return;
            }

            if (MessageBox.Show("Удалить товар?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            _db.Tovar.Remove(entity);
            _db.SaveChanges();

            LoadProducts();
        }

  
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
 

        public class ProductView
        {
            public int Id { get; set; }
            public string CategoryName { get; set; }
            public string TovarName { get; set; }
            public string Description { get; set; }
            public string Manufacturer { get; set; }
            public string Supplier { get; set; }
            public decimal Cost { get; set; }
            public decimal FinalPrice { get; set; }
            public int Discount { get; set; }
            public string Unit { get; set; }
            public int Stock { get; set; }
            public string ImagePath { get; set; }
            public Brush RowBrush { get; set; }
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow();
            orderWindow.Show();

        }
    }
}