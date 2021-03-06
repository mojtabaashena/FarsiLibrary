﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using FarsiLibrary.Localization;
using FarsiLibrary.Utils;
using FarsiLibrary.Win.BaseClasses;
using FarsiLibrary.Win.Design;
using FarsiLibrary.Win.Enums;
using FarsiLibrary.Win.Events;

namespace FarsiLibrary.Win.Controls
{
    /// <summary>
    /// A datepicker control which can select date in <see cref="System.Globalization.GregorianCalendar"/>, <see cref="PersianCalendar" /> and <see cref="System.Globalization.HijriCalendar"/> based on current thread's Culture and UICulture. 
    /// 
    /// To know how to display the control in other cultures and calendars, please see <see cref="FAMonthView"/> control's documentation.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent("SelectedDateTimeChanged")]
    [DefaultProperty("SelectedDateTime")]
    [Designer(typeof(FADatePickerDesigner))]
    [DefaultBindingProperty("SelectedDateTime")]
    public class FADatePicker : FAContainerComboBox
    {
        #region Fields

        private DateTime? selectedDateTime;
        private string dateseparator = ";";
        internal FAMonthViewContainer mv;

	    #endregion

        #region Events

        /// <summary>
        /// Fires when SelectedDateTime property of the control changes.
        /// </summary>
        public event EventHandler SelectedDateTimeChanged;

        /// <summary>
        /// Fires when SelectedDateTime property of the control is changing.
        /// </summary>
        public event SelectedDateTimeChangingEventHandler SelectedDateTimeChanging;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new instance of FADatePicker class.
        /// </summary>
        public FADatePicker()
        {
            mv = new FAMonthViewContainer(this);
            RightToLeftChanged += OnInternalRightToLeftChanged;
            mv.MonthViewControl.SelectedDateTimeChanged += OnMVSelectedDateTimeChanged;
            mv.MonthViewControl.SelectedDateRangeChanged += OnMVSelectedDateRangeChanged;
            mv.MonthViewControl.ButtonClicked += OnMVButtonClicked;
            FALocalizeManager.Instance.LocalizerChanged += OnInternalLocalizerChanged;
            base.TextBox.TextChanged += (sender, e) => OnTextChanged(EventArgs.Empty);
            PopupShowing += OnInternalPopupShowing;
            Text = FALocalizeManager.Instance.GetLocalizerByCulture(mv.MonthViewControl.DefaultCulture).GetLocalizedString(StringID.Validation_NullText);
            FormatInfo = FormatInfoTypes.ShortDate;
        }

        #endregion

        #region Props
        
        /// <summary>
        /// Determines if the control has not made any selection yet.
        /// </summary>
        [DefaultValue(true)]
        [Description("Determines if the control has not made any selection yet.")]
        [Browsable(false)]
        public bool IsNull
        {
            get { return mv.MonthViewControl.IsNull; }
        }

        /// <summary>
        /// Determinces scrolling option of the MonthView control.
        /// </summary>
        [DefaultValue(typeof(ScrollOptionTypes), "Month")]
        [Description("Determinces scrolling option of the MonthView control.")]
        public ScrollOptionTypes ScrollOption
        {
            get { return mv.MonthViewControl.ScrollOption; }
            set { mv.MonthViewControl.ScrollOption = value; }
        }

        /// <summary>
        /// Determines if Empty button should be shown in MonthView control.
        /// </summary>
        [DefaultValue(true)]
        [Description("Determines if Empty button should be shown in MonthView control")]
        [RefreshProperties(RefreshProperties.All)]
        public bool ShowEmptyButton
        {
            get { return mv.MonthViewControl.ShowEmptyButton; }
            set { mv.MonthViewControl.ShowEmptyButton = value; }
        }

        /// <summary>
        /// Determines if Today button should be shown in MonthView control.
        /// </summary>
        [DefaultValue(true)]
        [Description("Determines if Today button should be shown in MonthView control")]
        [RefreshProperties(RefreshProperties.All)]
        public bool ShowTodayButton
        {
            get { return mv.MonthViewControl.ShowTodayButton; }
            set { mv.MonthViewControl.ShowTodayButton = value; }
        }
        
        /// <summary>
        /// Gets or Sets to show a border around the MonthView control.
        /// </summary>
        [DefaultValue(true)]
        [Description("Gets or Sets to show a border around the MonthView control.")]
        public bool ShowBorder
        {
            get { return mv.MonthViewControl.ShowBorder; }
            set { mv.MonthViewControl.ShowBorder = value; }
        }

        /// <summary>
        /// Gets or Sets to show the focus rectangle around the selected day in MonthView control.
        /// </summary>
        [DefaultValue(false)]
        [Description("Gets or Sets to show the focus rectangle around the selected day in MonthView control.")]
        public bool ShowFocusRect
        {
            get { return mv.MonthViewControl.ShowFocusRect; }
            set { mv.MonthViewControl.ShowFocusRect = value; }
        }

        /// <summary>
        /// Selected value of the control as a <see cref="DateTime"/> instance.
        /// </summary>
        [Bindable(true)]
        [Localizable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [Description("Selected value of the control as a DateTime instance.")]
        public DateTime? SelectedDateTime
        {
            get { return selectedDateTime; }
            set
            {
                if (selectedDateTime == value)
                    return;

                //Validating
                var validateArgs = new ValueValidatingEventArgs(Text) {HasError = HasErrors};
                OnValueValidating(validateArgs);
                if (validateArgs.HasError)
                    return;

                var oldValue = selectedDateTime;
                var newValue = value;
                var changeArgs = new SelectedDateTimeChangingEventArgs(newValue, oldValue);
                OnSelectedDateTimeChanging(changeArgs);

                if (changeArgs.Cancel)
                {
                    if (string.IsNullOrEmpty(changeArgs.Message))
                        Error.SetError(this, FALocalizeManager.Instance.GetLocalizer().GetLocalizedString(StringID.Validation_Cancel));
                    else
                        Error.SetError(this, changeArgs.Message);

                    return;
                }

                if (!string.IsNullOrEmpty(changeArgs.Message))
                {
                    Error.SetError(this, changeArgs.Message);
                }
                else
                {
                    Error.SetError(this, string.Empty);
                }

                //No errors, proceed
                mv.MonthViewControl.SelectedDateTime = changeArgs.NewValue;
            }
        }

        /// <summary>
        /// Selected values collection, if the control is in MultiSelect mode.
        /// </summary>
        [Editor(typeof(DateTimeCollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(DateTimeConverter))]
        [Description("Selected values collection, if the MonthView control is in MultiSelect mode.")]
        public DateTimeCollection SelectedDateRange
        {
            get { return mv.MonthViewControl.SelectedDateRange; }
        }

        /// <summary>
        /// Gets or Sets the control in MultiSelect mode.
        /// </summary>
		[DefaultValue(false)]
        [Description("Gets or Sets the control in MultiSelect mode.")]
		public bool IsMultiSelect
		{
			get { return mv.MonthViewControl.IsMultiSelect; }
			set
			{
			    mv.MonthViewControl.IsMultiSelect = value;
                UpdateTextValue();
			}
		}

        /// <summary>
        /// Gets or Sets the character that separates date values when control 
        /// is in MultiSelect mode.
        /// </summary>
        [DefaultValue(";")]
        [Description("Gets or Sets the character that separates date values when control is in MultiSelect mode.")]
        public string DateSeparator
        {
            get { return dateseparator; }
            set { dateseparator = value; }
        }

        #endregion

        #region EventHandling

        private void OnInternalLocalizerChanged(object sender, EventArgs e)
        {
            UpdateTextValue();
        }

        private void OnInternalRightToLeftChanged(object sender, EventArgs e)
        {
            SetPosTextBox();
        }

        private void OnInternalPopupShowing(object sender, EventArgs e)
        {
            mv.MonthViewControl.Theme = Theme;
            var args = new ValueValidatingEventArgs(Text);
            OnValueValidating(args);
        }

        protected override void OnBindingPopupControl(BindPopupControlEventArgs e)
        {
            e.BindedControl = mv;
            base.OnBindingPopupControl(e);
        }

        protected virtual void OnSelectedDateTimeChanging(SelectedDateTimeChangingEventArgs e)
        {
            e.Cancel = false;

            if (SelectedDateTimeChanging != null)
                SelectedDateTimeChanging(this, e);
        }

        protected virtual void OnSelectedDateTimeChanged(EventArgs e)
        {
            if (SelectedDateTimeChanged != null)
                SelectedDateTimeChanged(this, e);
        }

        private void OnMVSelectedDateTimeChanged(object sender, EventArgs e)
        {
            SetSelectedDateTime(mv.MonthViewControl.SelectedDateTime);
        }

        private void OnMVSelectedDateRangeChanged(object sender, SelectedDateRangeChangedEventArgs e)
        {
            UpdateTextValue();
        }

        private void OnMVButtonClicked(object sender, CalendarButtonClickedEventArgs e)
        {
            HideDropDown();
        }

        private void SetSelectedDateTime(DateTime? dt)
        {
            var oldValue = selectedDateTime;
            var newValue = dt;
            
            var changeArgs = new SelectedDateTimeChangingEventArgs(newValue, oldValue);
            OnSelectedDateTimeChanging(changeArgs);
            
            if (changeArgs.Cancel)
            {
                if(string.IsNullOrEmpty(changeArgs.Message))
                {
                    Error.SetError(this, FALocalizeManager.Instance.GetLocalizer().GetLocalizedString(StringID.Validation_Cancel));
                }
                else
                {
                    Error.SetError(this, changeArgs.Message);
                }

                return;
            }
            
            if(!string.IsNullOrEmpty(changeArgs.Message))
            {
                Error.SetError(this, changeArgs.Message);
            }
            else
            {
                Error.SetError(this, string.Empty);
            }

            selectedDateTime = changeArgs.NewValue;
            OnSelectedDateTimeChanged(EventArgs.Empty);

            UpdateTextValue();
        }

        /// <summary>
        /// Updates text representation of the selected value
        /// </summary>
        public override void UpdateTextValue()
        {
            if (mv.MonthViewControl.IsNull)
            {
                Text = FALocalizeManager.Instance.GetLocalizerByCulture(mv.MonthViewControl.DefaultCulture).GetLocalizedString(StringID.Validation_NullText);
            }
            else
            {
                if(!IsMultiSelect)
                {
                    Text = ConvertDateValue(SelectedDateTime);
                }
                else
                {
                    string textValue = string.Empty;
                    bool isFirst = true;
                    foreach (var date in SelectedDateRange)
                    {
                        if(!isFirst)
                        {
                            textValue += DateSeparator;
                        }

                        textValue += ConvertDateValue(date);
                        isFirst = false;
                    }

                    Text = textValue;
                }
            }
        }

        private string ConvertDateValue(DateTime? date)
        {
            string result;

            if(!date.HasValue)
            {
                result = FALocalizeManager.Instance.GetLocalizer().GetLocalizedString(StringID.Validation_NullText);
            }
            else if (mv.MonthViewControl.DefaultCulture.Equals(mv.MonthViewControl.PersianCulture))
            {
                result = ((PersianDate) date).ToString(GetFormatByFormatInfo(FormatInfo));
            }
            else
            {
                result = date.Value.ToString(GetFormatByFormatInfo(FormatInfo), mv.MonthViewControl.DefaultCulture);
            }

            return result;
        }

        protected override void OnValidating(CancelEventArgs e)
        {
            var args = new ValueValidatingEventArgs(Text);
            OnValueValidating(args);
            e.Cancel = args.HasError;

            base.OnValidating(e);
        }

        protected override void OnValueValidating(ValueValidatingEventArgs e)
        {
            base.OnValueValidating(e);

            try
            {
                var txt = e.Value;
                if (string.IsNullOrEmpty(txt) || txt == FALocalizeManager.Instance.GetLocalizer().GetLocalizedString(StringID.Validation_NullText))
                {
                    e.HasError = false;
                }
                else
                {
                    if(!IsMultiSelect)
                    {
                        var pd = Parse(txt);
                        e.HasError = false;
                        mv.MonthViewControl.SelectedDateTime = pd;
                    }
                    else
                    {
                        var dates = txt.Split(DateSeparator.ToCharArray(0, 1));
                        var dateList = new List<DateTime>();

                        foreach (string dateEntry in dates)
                        {
                            var pd = Parse(dateEntry);
                            dateList.Add(pd);
                        }

                        e.HasError = false;
                        mv.MonthViewControl.SelectedDateRange.Clear();
                        mv.MonthViewControl.SelectedDateRange.AddRange(dateList.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                e.HasError = true;
                mv.MonthViewControl.SelectedDateTime = null;
            }
        }

        private DateTime Parse(string value)
        {
            if (mv.MonthViewControl.DefaultCulture.Equals(mv.MonthViewControl.PersianCulture))
            {
                return PersianDate.Parse(value);
            }
            else
            {
                return DateTime.Parse(value);
            }
        }
        
        #endregion

        #region ShouldSerialize and Reset

        /// <summary>
        /// Decides to serialize the SelectedDateTime property or not.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeSelectedDateTime()
        {
            return SelectedDateTime.HasValue;
        }

        /// <summary>
        /// Rests SelectedDateTime to default value.
        /// </summary>
        public void ResetSelectedDateTime()
        {
            SelectedDateTime = null;
        }

        #endregion

        #region Overrides

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (mv.Visible == false && (this.FormatInfo == FormatInfoTypes.DateShortTime || this.FormatInfo == FormatInfoTypes.ShortDate))
            {
                //if DropDown is not visible , mouse scroll will change selected part of date
                AddOrDecreaseDate(e.Delta / 120);
            }
            base.OnMouseWheel(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                if (this.IsReadonly || this.FormatInfo == FormatInfoTypes.FullDateTime || this.SelectedDateTime == null )
                {
                    base.OnMouseDown(e);
                    return;
                }

                // Select part of date based on the mouse position
                if (((this.SelectionStart < 5) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 0;
                    this.SelectionLength = 4;
                }
                else if (((this.SelectionStart < 8) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 5;
                    this.SelectionLength = 2;
                }
                else if (((this.SelectionStart < 11) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 8;
                    this.SelectionLength = 2;
                }
                else if (((this.SelectionStart < 14) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 11;
                    this.SelectionLength = 2;
                }
                else if (((this.SelectionStart < 17) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 14;
                    this.SelectionLength = 2;
                }
                else if (((this.SelectionStart < 20) && (this.SelectionLength == 0)))
                {
                    this.SelectionStart = 17;
                    this.SelectionLength = 3;
                }

            }
            catch { }
            base.OnMouseDown(e);

        }

        public void AddOrDecreaseDate(int intValue)
        {

            try
            {
                FarsiLibrary.Utils.PersianDate fdt = new FarsiLibrary.Utils.PersianDate();
                fdt.Year = Convert.ToInt32(this.Text.Substring(0, 4));
                fdt.Month = Convert.ToInt32(this.Text.Substring(5, 2));
                fdt.Day = Convert.ToInt32(this.Text.Substring(8, 2));
                if ((this.SelectionStart < 5))
                {
                    // Year
                    if (((fdt.Month == 11) && ((intValue > 0) && (fdt.Day > 29))))
                    {
                        fdt.Day = 29;
                        this.Text = fdt.ToString("d");
                    }

                    string str = Convert.ToString((fdt.Year + intValue));
                    this.Text = this.Text.Remove(0, 4).Insert(0, str.Substring((str.Length - 4)));
                    this.SelectionStart = 0;
                    this.SelectionLength = 4;
                }
                else if ((this.SelectionStart < 8))
                {
                    // Month
                    int intMonth = (fdt.Month + intValue);
                    Math.DivRem(intMonth, 12, out intMonth);
                    if ((intMonth == 0))
                    {
                        intMonth = 12;
                    }
                    else if ((intMonth < 0))
                    {
                        intMonth = Math.Abs(intMonth);
                    }

                    if (((intMonth == 12)
                                && (fdt.Day > 29)))
                    {
                        fdt.Day = 29;
                    }
                    else if (((intMonth > 6)
                                && (fdt.Day == 31)))
                    {
                        fdt.Day = 30;
                    }

                    fdt.Month = intMonth;
                    this.Text = this.Text.Remove(0, 10).Insert(0, fdt.ToString("d"));
                    this.SelectionStart = 5;
                    this.SelectionLength = 2;
                }
                else if ((this.SelectionStart < 11))
                {
                    // Day
                    this.Text = this.Text.Remove(0, 10).Insert(0, FarsiLibrary.Utils.PersianDateConverter.ToPersianDate(fdt.ToDateTime().AddDays(intValue)).ToString("d"));
                    this.SelectionStart = 8;
                    this.SelectionLength = 2;
                }
                else if ((this.SelectionStart < 14))
                {
                    // Hour
                    int intHour = Convert.ToInt32(this.Text.Substring(11, 2));
                    intHour = (intHour + intValue);
                    intHour = (intHour > 12) ? 1 : (intHour < 1) ? 12 : intHour;

                    this.Text = this.Text.Remove(11, 2).Insert(11, string.Format("{0:00}", intHour));
                    this.SelectionStart = 11;
                    this.SelectionLength = 2;
                }
                else if ((this.SelectionStart < 17))
                {
                    int intMinute = Convert.ToInt32(this.Text.Substring(14, 2));
                    intMinute = (intMinute - (intMinute % 5));
                    intMinute = (intMinute + (intValue * 5));
                    intMinute = (intMinute >= 60) ? 0 : (intMinute < 0) ? 55 : intMinute;

                    this.Text = this.Text.Remove(14, 2).Insert(14, string.Format("{0:00}", intMinute));
                    this.SelectionStart = 14;
                    this.SelectionLength = 2;
                }
                else if ((this.SelectionStart <= 20))
                {
                    this.Text = this.Text.Remove(17, 3).Insert(17, (this.Text.Contains("ب.ظ") ? "ق.ظ" : "ب.ظ"));
                    this.SelectionStart = 17;
                    this.SelectionLength = 3;
                }

            }
            catch { }


        }

        protected override void OnLeave(EventArgs e)
        {
            try
            {
                //Auto Correct Date on Leave in case user type the date in TextBox
                //Example : 97/1/1 -> 1397/01/01 
                if (this.IsReadonly || this.FormatInfo == FormatInfoTypes.FullDateTime || this.SelectedDateTime == null)
                {
                    base.OnLeave(e);
                    return;
                }

                if (((this.Text.Length != 10) && (this.Text != "")))
                {
                    string[] strInputDate;
                    strInputDate = this.Text.Split(new char[] { '/' });
                    if ((strInputDate[0].Length == 2))
                    {
                        strInputDate[0] = (FarsiLibrary.Utils.PersianDate.Now.Year.ToString().Substring(0, 2) + strInputDate[0]);
                    }

                    if ((strInputDate[0].Length != 4))
                    {
                        strInputDate[0] = FarsiLibrary.Utils.PersianDate.Now.Year.ToString();
                    }

                    if ((strInputDate[1].Length == 1))
                    {
                        strInputDate[1] = ("0" + strInputDate[1]);
                    }

                    if ((strInputDate[1].Length > 2))
                    {
                        strInputDate[1] = "01";
                    }

                    if (((Convert.ToInt32(strInputDate[1]) > 12) || (Convert.ToInt32(strInputDate[1]) < 1)))
                    {
                        strInputDate[1] = "12";
                    }

                    if ((strInputDate[2].Length == 1))
                    {
                        strInputDate[2] = ("0" + strInputDate[2]);
                    }

                    if ((strInputDate[2].Length > 2))
                    {
                        strInputDate[2] = "01";
                    }

                    if (((Convert.ToInt32(strInputDate[2]) > 31) || (Convert.ToInt32(strInputDate[2]) < 1)))
                    {
                        strInputDate[2] = "01";
                    }

                    this.Text = (strInputDate[0] + ("/" + (strInputDate[1] + ("/" + strInputDate[2]))));
                }

            }
            catch
            {
                if (this.Text != "")
                    this.Text = FarsiLibrary.Utils.PersianDate.Now.ToString("d");
            }
            base.OnLeave(e);
        }

        #endregion
    }
}
