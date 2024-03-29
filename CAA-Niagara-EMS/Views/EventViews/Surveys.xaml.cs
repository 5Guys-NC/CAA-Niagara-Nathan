﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CAA_Event_Management.Data;
using CAA_Event_Management.Models;
using Windows.UI.Xaml.Media.Animation;
/********************************
* Created By: Jon Yade
* Edited By:
* ******************************/

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace CAA_Event_Management.Views.EventViews
{
    /// <summary>
    /// Frame for Items
    /// </summary>
    public sealed partial class Surveys : Page
    {
        #region Startup - variables, respositories, methods

        Item item;
        Item selectedItem;
        int addOrEdit;
        readonly List<DataType> dataList = new List<DataType>();

        IItemRepository itemRespository;

        public Surveys()
        {
            this.InitializeComponent();
            itemRespository = new ItemRepository();

            FillDataTypeComboBox();
            FillFields();
        }
          
        #endregion

        #region Buttons - Add, Edit, Delete Questions

        private void btnAddSurveyQuestion_Click(object sender, RoutedEventArgs e)
        {
            ScreenLockDown();
            tbkEnterQuestion.Visibility = Visibility.Visible;
            txtNewSurveyQuestion.Visibility = Visibility.Visible;
            tbkDataType.Visibility = Visibility.Visible;
            cboDataType.Visibility = Visibility.Visible;
            rpSaveAndCancel.Visibility = Visibility.Visible;

            addOrEdit = 1;
        }

        private async void btnEditMultipurpose_Click(object sender, RoutedEventArgs e)
        {
            addOrEdit = 2;

            //Item selectedItem = (Item)lstPreMadeQuestions.SelectedItem;
            selectedItem = (Item)gvAvailableQuestions.SelectedItem;
            string warning = "Please exercise caution when editing this question. This question may " +
                "have been used in events and may have saved results.  Thus, changing the question and " +
                "datatypes may cause the program to become unstable and make previously collected data useless. " +
                "Do you wish to continue?";

            if (selectedItem != null)
            {
                var result = await Jeeves.ConfirmDialog("Warning", warning);

                if (result == ContentDialogResult.Secondary && btnEditQuestion.Content.ToString() == "Edit Question")
                {
                    tbkEnterQuestion.Visibility = Visibility.Visible;
                    txtNewSurveyQuestion.Visibility = Visibility.Visible;
                    tbkDataType.Visibility = Visibility.Visible;
                    cboDataType.Visibility = Visibility.Visible;
                    rpSaveAndCancel.Visibility = Visibility.Visible;
                    BeginUpdate(selectedItem);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    SaveQuestion(selectedItem);
                }
            }
        }

        private void btnSaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (addOrEdit == 1)
            {

                item = new Item();
                this.DataContext = item;

                try
                {
                    if (cboDataType.SelectedItem == null)
                    {
                        Jeeves.ShowMessage("Error", "Please select a data type");
                        return;
                    }
                    else if (txtNewSurveyQuestion.Text != "")
                    {
                        DataType selectedDataType = (DataType)cboDataType.SelectedItem;

                        item.ItemID = Guid.NewGuid().ToString();
                        item.ItemName = (string)txtNewSurveyQuestion.Text;
                        item.ValueType = selectedDataType.DisplayText;
                        itemRespository.AddItem(item);
                        ClearFields();
                    }

                }
                catch (Exception ex)
                {
                    Jeeves.ShowMessage("Error", ex.GetBaseException().Message.ToString());
                }
                addOrEdit = 0;
                tbkEnterQuestion.Visibility = Visibility.Collapsed;
                txtNewSurveyQuestion.Visibility = Visibility.Collapsed;
                tbkDataType.Visibility = Visibility.Collapsed;
                cboDataType.Visibility = Visibility.Collapsed;
                rpSaveAndCancel.Visibility = Visibility.Collapsed;
                ScreenUnlock();
                ClearFields();
                FillFields();
            }
            else
            {
                tbkEnterQuestion.Visibility = Visibility.Collapsed;
                txtNewSurveyQuestion.Visibility = Visibility.Collapsed ;
                tbkDataType.Visibility = Visibility.Collapsed;
                cboDataType.Visibility = Visibility.Collapsed;
                rpSaveAndCancel.Visibility = Visibility.Collapsed; ;
                SaveQuestion(selectedItem);
                selectedItem = null;
                addOrEdit = 0;
            }

        }

        private void btnCancelSave_Click(object sender, RoutedEventArgs e)
        {
            tbkEnterQuestion.Visibility = Visibility.Collapsed;
            txtNewSurveyQuestion.Visibility = Visibility.Collapsed;
            tbkDataType.Visibility = Visibility.Collapsed;
            cboDataType.Visibility = Visibility.Collapsed;
            rpSaveAndCancel.Visibility = Visibility.Collapsed;
            selectedItem = null;
            ScreenUnlock();
            ClearFields();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (btnDelete.Content.ToString() == "Delete Mode (OFF)")
            {
                btnDelete.Content = "Delete Mode (ON)";
                rpSurvey.Visibility = Visibility.Collapsed;
                rpSurveyDeleteMode.Visibility = Visibility.Visible;
                FillFields();
            }
            else
            {
                btnDelete.Content = "Delete Mode (OFF)";
                rpSurvey.Visibility = Visibility.Visible;
                rpSurveyDeleteMode.Visibility = Visibility.Collapsed;
                FillFields();
            }
        }
    

        private void BtnConfirmRemove_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string selected = Convert.ToString(((Button)sender).DataContext);
            //Item thisSelectedEvent = new Item();
            Item thisSelectedItem = itemRespository.GetItem(selected.ToString());
            itemRespository.DeleteItem(thisSelectedItem);
            Frame.Navigate(typeof(Surveys), null, new SuppressNavigationTransitionInfo());
        }

        private void BtnCancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            return;
        }

        private void btnMostUsedQuestions_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(btnMostUsedQuestions, 1);
            Canvas.SetZIndex(btnAlphabeticalQuestions, 0);
        }

        private void btnAlphabeticalQuestions_Click(object sender, RoutedEventArgs e)
        {
            Canvas.SetZIndex(btnAlphabeticalQuestions, 1);
            Canvas.SetZIndex(btnMostUsedQuestions, 0);
        }

        #endregion

        #region Helper Methods - FillFields, FillDataTypeComboBox, SearchField, BeginUpdate, SaveQuestion, ClearFields

        private void FillFields()
        {
            try
            {
                List<Item> items = itemRespository.GetItems();
                //lstPreMadeQuestions.ItemsSource = items;
                gvAvailableQuestions.ItemsSource = items;
                gvAvailableQuestionsDeleteMode.ItemsSource = items;
            }
            catch (Exception)
            {
                Jeeves.ShowMessage("Error", "There was an error in retreving the questions");
            }
        }

        private void FillDataTypeComboBox()
        {
            DataType dt1 = new DataType();
            DataType dt2 = new DataType();
            DataType dt3 = new DataType();

            dt1.DisplayText = "Yes-No";
            dt1.Type = "yesNo";

            dt2.DisplayText = "Numbers";
            dt2.Type = "number";

            dt3.DisplayText = "Words";
            dt3.Type = "word";

            dataList.Add(dt1);
            dataList.Add(dt2);
            dataList.Add(dt3);
            cboDataType.ItemsSource = dataList;
        }

        private void txtSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearchBox.Text == "") FillFields();
            else
            {
                DataType dataTypes = new DataType();

                try
                {
                    List<Item> items = itemRespository.GetItems();
                    List<Item> searchItems = new List<Item>();
                    string searchString = txtSearchBox.Text.ToLower();

                    foreach (var x in items)
                    {
                        if (x.ItemName.ToLower().IndexOf(searchString) > -1)
                        {
                            searchItems.Add(x);
                        }
                    }
                    //lstPreMadeQuestions.ItemsSource = searchItems;
                    gvAvailableQuestions.ItemsSource = searchItems;
                }
                catch (Exception)
                {
                    Jeeves.ShowMessage("Error", "There was an error in retreving the questions");
                }
            }
        }

        private void BeginUpdate(Item selectedItem)
        {
            DataType selectedDataType = dataList
                .Where(c => c.DisplayText == selectedItem.ValueType)
                .FirstOrDefault();
            cboDataType.SelectedItem = selectedDataType;
            txtNewSurveyQuestion.Text = selectedItem.ItemName;
            ScreenLockDown();
        }

        private void SaveQuestion(Item selectedItem)
        {
            try
            {
                selectedItem.ItemName = txtNewSurveyQuestion.Text;
                DataType selectedDataType = (DataType)cboDataType.SelectedItem;

                selectedItem.ValueType = selectedDataType.DisplayText;
                itemRespository.UpdateItem(selectedItem);

                ScreenUnlock();
                ClearFields();
            }
            catch
            {
                Jeeves.ShowMessage("Error", "Unable to save the question");
            }
            FillFields();
        }

        private void ClearFields()
        {
            txtNewSurveyQuestion.Text = "";
            txtSearchBox.Text = "";
            cboDataType.SelectedItem = null;
        }

        private void ScreenLockDown()
        {
            btnEditQuestion.IsEnabled = false;
            btnAddSurveyQuestion.IsEnabled = false;
            btnDelete.IsEnabled = false;
            gvAvailableQuestions.IsEnabled = false;
        }

        private void ScreenUnlock()
        {
            btnAddSurveyQuestion.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnEditQuestion.IsEnabled = true;
            gvAvailableQuestions.IsEnabled = true;
        }

        #endregion

    }

    internal class DataType
    {
        internal int ID { get; set; }
        public string DisplayText { get; set; }
        internal string Type { get; set; }
    }
}