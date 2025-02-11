﻿using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace yawola
{
	public sealed partial class MainPage : Page
	{
		/// <summary>
		/// an array holding references to all fields in the <see cref="addHostDialog"/> to enable easier removal of inputted text wehn the user cancels the operation
		/// </summary>
		private readonly TextBox[] popupFields = new TextBox[9];

		private bool editing = false;

		public DummyData data = new DummyData();
		/// <summary>
		/// The constructor called by the system when MainPage is to be displayed. Initializes the page and performs any preperation necessary
		/// </summary>
		public MainPage()
		{
			InitializeComponent();
			//fill the popupFields array with references to all input fields in the addHostDialog
			popupFields[0] = clientNameInput;
			popupFields[1] = ipInput;
			popupFields[2] = macInput0;
			popupFields[3] = macInput1;
			popupFields[4] = macInput2;
			popupFields[5] = macInput3;
			popupFields[6] = macInput4;
			popupFields[7] = macInput5;
			popupFields[8] = portInput;
		}

		/// <summary>
		/// Creates a new instance of WolTarget and adds it to <see cref="AppData.targets"/>
		/// </summary>
		private void Add_Host()
		{
			WolTarget target = CreateHostObject();
			AppData.targets.Add(target);
			if (AppData.debug)
				Debug.WriteLine("Saving entry no " + AppData.targets.IndexOf(target));
			if (AppData.debug)
			{
				Debug.WriteLine("Saved target:");
				Debug.WriteLine(target.Address);
				Debug.WriteLine(target.Mac_string);
				Debug.WriteLine(target.Port);
			}
		}
		/// <summary>
		/// Creates a new instance of WolTarget and replaces the one currently selected in <see cref="TargetList"/>
		/// </summary>
		private void UpdateHost()
		{
			WolTarget target = (WolTarget)TargetList.SelectedItem;
			//create copy of old WolTarget objet to roll back to in case something goes wrong
			WolTarget old = new WolTarget(target);
			int index = AppData.targets.IndexOf(target);
			if (AppData.debug)
				Debug.WriteLine("Saving entry no " + index);
			//update entry in a try block so changes can be rolled back in the catch block if something goes wrong
			try
			{
				target.SetAddress(ipInput.Text);
				string[] mac = {
					macInput0.Text, macInput1.Text ,
					macInput2.Text, macInput3.Text ,
					macInput4.Text, macInput5.Text
				};
				target.SetMac(mac);
				target.SetPort(portInput.Text);
				target.Name = clientNameInput.Text;

			}
			catch (Exception e)
			{
				Debug.WriteLine("Exception ocurred while updating targetList entry. Rolling back...");
				Debug.WriteLine(e.Message);
				//add the old WolTarget object back into the list
				_ = AppData.targets.Remove(target);
				AppData.targets.Insert(index, old);
				TargetList.SelectedIndex = index;
				return;
			}

			if (AppData.debug)
			{
				Debug.WriteLine("Saved target:");
				Debug.WriteLine(target.Address);
				Debug.WriteLine(target.Mac_string);
				Debug.WriteLine(target.Port);
			}
		}
		/// <summary>
		/// Creates a new instance of WolTarget and returns it
		/// </summary>
		/// <returns>The Woltarget generated from the <see cref="addHostDialog"/></returns>
		private WolTarget CreateHostObject()
		{
			string[] mac = {
				macInput0.Text, macInput1.Text ,
				macInput2.Text, macInput3.Text ,
				macInput4.Text, macInput5.Text
			};
			return new WolTarget(ipInput.Text, mac, clientNameInput.Text, portInput.Text);
		}
		/// <summary>
		/// Callback for the "Edit" button. Opens the addHostDialog prefilled with the entry selected from <see cref="AppData.targets"/> for editing.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Edit_Button_Click(object sender, RoutedEventArgs e)
		{
			editing = true;
			WolTarget t = (WolTarget)TargetList.SelectedItem;
			if (AppData.debug)
				Debug.WriteLine("Editing entry no " + AppData.targets.IndexOf(t));
			clientNameInput.Text = t.Name;
			ipInput.Text = t.Address;
			portInput.Text = t.Port;
			macInput0.Text = t.Mac_string_array[0];
			macInput1.Text = t.Mac_string_array[1];
			macInput2.Text = t.Mac_string_array[2];
			macInput3.Text = t.Mac_string_array[3];
			macInput4.Text = t.Mac_string_array[4];
			macInput5.Text = t.Mac_string_array[5];
			_ = await addHostDialog.ShowAsync();
		}

		/// <summary>
		/// Callback for the "Delete" button. Removes the selected entry from <see cref="AppData.targets"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Delete_Button_Click(object sender, RoutedEventArgs e)
		{
			_ = AppData.targets.Remove((WolTarget)TargetList.SelectedItem);
		}

		/// <summary>
		/// Callback for the "Add" button. Displays <see cref="addHostDialog"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private async void Open_Add_Dialog(object sender, RoutedEventArgs e)
		{
			_ = await addHostDialog.ShowAsync();
		}

		/// <summary>
		/// Validates the inputs in <see cref="addHostDialog"/> and enbles its primaryButton if the inputs are valid.<br/>
		/// The form is considered valid if the client name and address are not empty
		/// </summary>
		/// <remarks>
		/// The mac is not checked, since its correctness is suffieciently imposed by the input fields maxLength and Regex.<br/>
		/// Every field being blank will be parsed to 0x00:0x00:0x00:0x00:0x00:0x00, which is (theoretically) a valid mac.<br/>
		/// The same goes for the port (which just defaults to "0")</remarks>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ValidateAddHostForm(object sender = null, object args = null)
		{
			if (AppData.debug)
				Debug.WriteLine("Validating Form...");
			//host/ip and display name must be set
			if (clientNameInput.Text.Length > 0 && ipInput.Text.Length > 0)
			{
				addHostDialog.IsPrimaryButtonEnabled = true;
				if (AppData.debug)
					Debug.WriteLine("Validation successful");
				return;
			}
			addHostDialog.IsPrimaryButtonEnabled = false;
			if (AppData.debug)
				Debug.WriteLine("Validation failed");
		}
		/// <summary>
		/// Function to clear all fields in the addHostDialog. Is called by <see cref="Add_Host"/> and <see cref="AddHostDialog_CloseButtonClick"/>
		/// </summary>
		private void ClearDialogFields()
		{
			foreach (TextBox box in popupFields)
				box.Text = "";
		}
		/// <summary>
		/// Callback for the <see cref="addHostDialog"/>'s primary button. Clears all inputs made in the dialog and adds the new host to <see cref="AppData.targets"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void AddHostDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			if (editing)
				UpdateHost();
			else
				Add_Host();
			ClearDialogFields();
			editing = false;
		}

		/// <summary>
		/// Callback for the <see cref="addHostDialog"/>'s secondary button. Clears all inputs made in the dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void AddHostDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			ClearDialogFields();
		}

		/// <summary>
		/// Button callback to navigate to the settings page
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			_ = Frame.Navigate(typeof(SettingsPage));
		}

		/// <summary>
		/// Callbck for the <see cref="TargetList"/>'s ItemClicked event, orders the selected <see cref="WolTarget"/> to send a magic packet
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TargetList_ItemClick(object sender, ItemClickEventArgs e)
		{
			_ = ((WolTarget)e.ClickedItem).SendMagicPacket();
		}

		private void TargetList_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
		{
			ListView listView = (ListView)sender;
			targetContextFlyout.ShowAt(listView, e.GetPosition(listView));

			//if the user used right click to open the context menu, set selection to that item
			if (((FrameworkElement)e.OriginalSource).DataContext is WolTarget selectedItem)
				TargetList.SelectedIndex = AppData.targets.IndexOf(selectedItem);
			//if the keyboard context menu button was used, e.OriginalSource.DataKontext will be null. In that case, TargetList.selectedItem will be set and can be used instead
			else
			{
				//if selectedIndex is -1, the user has only navigated to the list and auto highlighted the first entry.
				//Selection would only happen once the user has navigated up or down inside the list. Thus we set the selectedIndex to 0
				if (TargetList.SelectedIndex == -1)
					TargetList.SelectedIndex = 0;
			}
		}
	}
}
