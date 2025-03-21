﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Diffusion.Toolkit.Models;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                name: nameof(Filter),
                propertyType: typeof(FilterControlModel),
                ownerType: typeof(FilterControl),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(
                name: nameof(SearchCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(FilterControl),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var search = d as FilterControl;

            if (e.Property.Name == nameof(Filter))
            {
                search.Filter.PropertyChanged += FilterOnPropertyChanged;

            }
        }

        public static readonly DependencyProperty ModelsProperty =
            DependencyProperty.Register(
                name: nameof(Models),
                propertyType: typeof(IEnumerable<string>),
                ownerType: typeof(FilterControl),
                typeMetadata: new UIPropertyMetadata(null)
            );

        public IEnumerable<string> Models
        {
            get => (IEnumerable<string>)GetValue(ModelsProperty);
            set => SetValue(ModelsProperty, value);
        }

        static PropertyInfo[] props = typeof(FilterControlModel).GetProperties();

        private static void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

            //if (!e.PropertyName.StartsWith("Use"))
            //{
            //    var prop = props.FirstOrDefault(x => x.Name == $"Use{e.PropertyName}");
            //    if (prop != null)
            //    {
            //        prop.SetValue(sender, true);
            //    }
            //}
        }

        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }


        public FilterControlModel Filter
        {
            get => (FilterControlModel)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public FilterControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PromptTextBox.Focus();
        }


        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            BindingExpression? binding = null;

            if (sender is TextBox textBox)
            {
                binding = textBox.GetBindingExpression(TextBox.TextProperty);
            }
            else
            {
                if (sender is ComboBox comboBox)
                {
                    binding = comboBox.GetBindingExpression(ComboBox.TextProperty);
                }
            }

            if (e.Key == Key.Escape)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                binding?.UpdateSource();

                if (SearchCommand.CanExecute(null))
                {
                    SearchCommand.Execute(null);
                }
            }
            else
            {
                var propertyName = binding.ResolvedSourcePropertyName;

                string checkBoxName = $"Use{propertyName}";

                switch (propertyName)
                {
                    case nameof(Filter.Height):
                    case nameof(Filter.Width):
                        checkBoxName = nameof(Filter.UseSize);
                        break;
                    case nameof(Filter.SeedStart):
                    case nameof(Filter.SeedEnd):
                        checkBoxName = nameof(Filter.UseSeed);
                        break;
                }

                var prop = props.FirstOrDefault(x => x.Name == checkBoxName);
                if (prop != null)
                {
                    prop.SetValue(Filter, true);
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as RadioButton;
            var binding = checkBox.GetBindingExpression(RadioButton.IsCheckedProperty);

            var propertyName = binding.ResolvedSourcePropertyName;

            if (propertyName.EndsWith("Op"))
            {
                propertyName = propertyName.Substring(0, propertyName.Length - 2);
            }

            string checkBoxName = $"Use{propertyName}";

            var prop = props.FirstOrDefault(x => x.Name == checkBoxName);
            if (prop != null)
            {
                prop.SetValue(Filter, true);
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter.UseModelName = true;
        }

        private void AddFilter_OnClick(object sender, RoutedEventArgs e)
        {
            Filter.AddNodeFilter();
        }
    }



}
