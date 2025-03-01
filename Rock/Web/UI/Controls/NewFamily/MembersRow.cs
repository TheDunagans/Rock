﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a bootstrap badge
    /// </summary>
    public class NewGroupMembersRow : CompositeControl
    {
        private RockRadioButtonList _rblRole;
        private DefinedValuePicker _dvpTitle;
        private RockTextBox _tbFirstName;
        private RockTextBox _tbNickName;
        private RockTextBox _tbMiddleName;
        private RockTextBox _tbLastName;
        private DefinedValuePicker _dvpSuffix;
        private DefinedValuePicker _dvpConnectionStatus;
        private RockRadioButtonList _rblGender;
        private DatePicker _dpBirthdate;
        private GradePicker _ddlGradePicker;

        private LinkButton _lbDelete;

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int? GroupTypeId
        {
            get { return ViewState["GroupTypeId"] as int?; }
            set { ViewState["GroupTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the person GUID.
        /// </summary>
        /// <value>
        /// The person GUID.
        /// </value>
        public Guid? PersonGuid
        {
            get
            {
                if ( ViewState["PersonGuid"] != null )
                {
                    return ( Guid ) ViewState["PersonGuid"];
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { ViewState["PersonGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the role id.
        /// </summary>
        /// <value>
        /// The role id.
        /// </value>
        public int? RoleId
        {
            get { return _rblRole.SelectedValueAsInt(); }
            set { SetListValue( _rblRole, value ); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show title]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTitle
        {
            get { return ViewState["ShowTitle"] as bool? ?? false; }
            set { ViewState["ShowTitle"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show suffix].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show suffix]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSuffix
        {
            get { return ViewState["ShowSuffix"] as bool? ?? false; }
            set { ViewState["ShowSuffix"] = value; }
        }

        /// <summary>
        /// Gets or sets the title value id.
        /// </summary>
        /// <value>
        /// The title value id.
        /// </value>
        public int? TitleValueId
        {
            get { return _dvpTitle.SelectedValueAsInt(); }
            set { SetListValue( _dvpTitle, value ); }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get { return _tbFirstName.Text; }
            set { _tbFirstName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the nick.
        /// </summary>
        /// <value>
        /// The name of the nick.
        /// </value>
        public string NickName
        {
            get { return _tbNickName.Text; }
            set { _tbNickName.Text = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show nick name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show nick name]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNickName { get; set; }

        /// <summary>
        /// Gets or sets the middle name
        /// </summary>
        /// <value>
        /// The name of the middle.
        /// </value>
        public string MiddleName
        {
            get { return _tbMiddleName.Text; }
            set { _tbMiddleName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get { return _tbLastName.Text; }
            set { _tbLastName.Text = value; }
        }

        /// <summary>
        /// Gets or sets the suffix value id.
        /// </summary>
        /// <value>
        /// The suffix value id.
        /// </value>
        public int? SuffixValueId
        {
            get { return _dvpSuffix.SelectedValueAsInt(); }
            set { SetListValue( _dvpSuffix, value ); }
        }

        /// <summary>
        /// Gets or sets the connection status value id.
        /// </summary>
        /// <value>
        /// The connection status value id.
        /// </value>
        public int? ConnectionStatusValueId
        {
            get { return _dvpConnectionStatus.SelectedValueAsInt(); }
            set { SetListValue( _dvpConnectionStatus, value ); }
        }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The gender.
        /// </value>
        public Gender Gender
        {
            get
            {
                return _rblGender.SelectedValueAsEnum<Gender>( Gender.Unknown );
            }
            set
            {
                SetListValue( _rblGender, value.ConvertToInt().ToString() );
            }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        public DateTime? BirthDate
        {
            get { return _dpBirthdate.SelectedDate; }
            set { _dpBirthdate.SelectedDate = value; }
        }

        /// <summary>
        /// Gets or sets the grade offset.
        /// </summary>
        /// <value>
        /// The grade offset.
        /// </value>
        public int? GradeOffset
        {
            get { return _ddlGradePicker.SelectedValueAsInt( noneAsNull: false ); }
            set { SetListValue( _ddlGradePicker, value ); }
        }

        /// <summary>
        /// Gets the group roles.
        /// </summary>
        /// <value>
        /// The group roles.
        /// </value>
        public List<GroupTypeRoleCache> GroupRoles
        {
            get
            {
                if ( GroupTypeId.HasValue )
                {
                    var groupGroupType = GroupTypeCache.Get( GroupTypeId.Value );
                    if ( groupGroupType != null )
                    {
                        return groupGroupType.Roles;
                    }
                }
                return new List<GroupTypeRoleCache>();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require gender].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get
            {
                return _rblGender.Required;
            }
            set
            {
                _rblGender.Required = value;
                BindGender();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireBirthdate
        {
            get
            {
                return _dpBirthdate.Required;
            }
            set
            {
                _dpBirthdate.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade column].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade column]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGradeColumn
        {
            get { return ViewState["ShowGradeColumn"] as bool? ?? false; }
            set { ViewState["ShowGradeColumn"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGradePicker
        {
            get { return ViewState["ShowGradePicker"] as bool? ?? false; }
            set { ViewState["ShowGradePicker"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show middle name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show middle name]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMiddleName
        {
            get { return ViewState["ShowMiddleName"] as bool? ?? false; }
            set { ViewState["ShowMiddleName"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require grade].
        /// </summary>
        /// <value>
        /// <c>true</c> if [require grade]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGrade
        {
            get
            {
                EnsureChildControls();
                return _ddlGradePicker.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlGradePicker.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return _tbFirstName.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _rblRole.ValidationGroup = value;
                _dvpTitle.ValidationGroup = value;
                _tbFirstName.ValidationGroup = value;
                _tbNickName.ValidationGroup = value;
                _tbMiddleName.ValidationGroup = value;
                _tbLastName.ValidationGroup = value;
                _dvpSuffix.ValidationGroup = value;
                _dvpConnectionStatus.ValidationGroup = value;
                _rblGender.ValidationGroup = value;
                _dpBirthdate.ValidationGroup = value;
                _ddlGradePicker.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGroupMembersRow" /> class.
        /// </summary>
        public NewGroupMembersRow()
            : base()
        {
            _rblRole = new RockRadioButtonList();
            _dvpTitle = new DefinedValuePicker();
            _tbFirstName = new RockTextBox();
            _tbNickName = new RockTextBox();
            _tbMiddleName = new RockTextBox();
            _tbLastName = new RockTextBox();
            _dvpSuffix = new DefinedValuePicker();
            _dvpConnectionStatus = new DefinedValuePicker();
            _rblGender = new RockRadioButtonList();
            _dpBirthdate = new DatePicker();
            _ddlGradePicker = new GradePicker { UseAbbreviation = true, UseGradeOffsetAsValue = true };
            _ddlGradePicker.Label = string.Empty;
            _lbDelete = new LinkButton();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _rblRole.ID = "_rblRole";
            _dvpTitle.ID = "_ddlTitle";
            _tbFirstName.ID = "_tbFirstName";
            _tbNickName.ID = "_tbNickName";
            _tbMiddleName.ID = "_tbMiddleName";
            _tbLastName.ID = "_tbLastName";
            _dvpSuffix.ID = "_ddlSuffix";
            _dvpConnectionStatus.ID = "_ddlConnectionStatus";
            _rblGender.ID = "_rblGender";
            _dpBirthdate.ID = "_dtBirthdate";
            _ddlGradePicker.ID = "_ddlGrade";
            _lbDelete.ID = "_lbDelete";

            Controls.Add( _rblRole );
            Controls.Add( _dvpTitle );
            Controls.Add( _tbFirstName );
            Controls.Add( _tbNickName );
            Controls.Add( _tbMiddleName );
            Controls.Add( _tbLastName );
            Controls.Add( _dvpSuffix );
            Controls.Add( _dvpConnectionStatus );
            Controls.Add( _rblGender );
            Controls.Add( _dpBirthdate );
            Controls.Add( _ddlGradePicker );
            Controls.Add( _lbDelete );

            _rblRole.RepeatDirection = RepeatDirection.Vertical;
            _rblRole.AutoPostBack = true;
            _rblRole.SelectedIndexChanged += rblRole_SelectedIndexChanged;
            _rblRole.Required = true;
            _rblRole.RequiredErrorMessage = "Role is required for all members";
            _rblRole.DataTextField = "Name";
            _rblRole.DataValueField = "Id";
            _rblRole.DataSource = GroupRoles;
            _rblRole.DataBind();

            _dvpTitle.CssClass = "form-control";
            _dvpTitle.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_TITLE ) ).Id;

            _tbFirstName.CssClass = "form-control";
            _tbFirstName.Placeholder = "First Name";
            _tbFirstName.Required = true;
            _tbFirstName.RequiredErrorMessage = "First Name is required for all group members";

            _tbNickName.CssClass = "form-control";
            _tbNickName.Placeholder = "Nick Name";

            _tbMiddleName.CssClass = "form-control";
            _tbMiddleName.Placeholder = "Middle Name";
            _tbMiddleName.Required = false;

            _tbLastName.CssClass = "form-control";
            _tbLastName.Placeholder = "Last Name";
            _tbLastName.Required = true;
            _tbLastName.RequiredErrorMessage = "Last Name is required for all group members";

            _dvpSuffix.CssClass = "form-control";
            _dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;

            _dvpConnectionStatus.CssClass = "form-control";
            _dvpConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;
            _dvpConnectionStatus.Required = true;
            _dvpConnectionStatus.RequiredErrorMessage = "Connection Status is required for all group members";

            _rblGender.RepeatDirection = RepeatDirection.Vertical;
            _rblGender.RequiredErrorMessage = "Gender is required for all group members";
            BindGender();

            _dpBirthdate.StartView = DatePicker.StartViewOption.decade;
            _dpBirthdate.ForceParse = false;
            _dpBirthdate.AllowFutureDateSelection = false;
            _dpBirthdate.RequiredErrorMessage = "Birthdate is required for all group members";
            _dpBirthdate.Required = false;

            _ddlGradePicker.CssClass = "form-control";
            _ddlGradePicker.RequiredErrorMessage = _ddlGradePicker.Label + " is required for all children";

            var iDelete = new HtmlGenericControl( "i" );
            _lbDelete.Controls.Add( iDelete );
            iDelete.AddCssClass( "fa fa-times" );

            _lbDelete.CssClass = "btn btn-square btn-danger pull-right";
            _lbDelete.Click += lbDelete_Click;
            _lbDelete.CausesValidation = false;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "rowid", ID );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _rblRole.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _rblRole.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                if ( this.ShowTitle )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    _dvpTitle.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Td );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbFirstName.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbFirstName.RenderControl( writer );
                writer.RenderEndTag();

                if ( this.ShowNickName )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbNickName.IsValid ? "" : " has-error" ) );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _tbNickName.RenderControl( writer );
                    writer.RenderEndTag();
                }

                if ( this.ShowMiddleName )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbMiddleName.IsValid ? "" : " has-error" ) );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _tbMiddleName.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _tbLastName.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbLastName.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();

                if ( this.ShowSuffix )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    _dvpSuffix.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _dvpConnectionStatus.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _rblGender.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _rblGender.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" + ( _dpBirthdate.IsValid ? "" : " has-error" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _dpBirthdate.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                if ( ShowGradeColumn )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    if ( ShowGradePicker )
                    {
                        _ddlGradePicker.RenderControl( writer );
                    }
                    writer.RenderEndTag();
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _lbDelete.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Binds the gender.
        /// </summary>
        private void BindGender()
        {
            string selectedValue = _rblGender.SelectedValue;

            _rblGender.Items.Clear();
            _rblGender.Items.Add( new ListItem( "M", "1" ) );
            _rblGender.Items.Add( new ListItem( "F", "2" ) );
            if ( !RequireGender )
            {
                _rblGender.Items.Add( new ListItem( "Unknown", "0" ) );
            }

            _rblGender.SelectedValue = selectedValue;
        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue( ListControl listControl, int? value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( value.HasValue && item.Value == value.Value.ToString() );
            }
        }

        /// <summary>
        /// Sets the list value.
        /// </summary>
        /// <param name="listControl">The list control.</param>
        /// <param name="value">The value.</param>
        private void SetListValue( ListControl listControl, string value )
        {
            foreach ( ListItem item in listControl.Items )
            {
                item.Selected = ( item.Value == value );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void rblRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( RoleUpdated != null )
            {
                RoleUpdated( this, e );
            }
        }


        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbDelete_Click( object sender, EventArgs e )
        {
            if ( DeleteClick != null )
            {
                DeleteClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [role updated].
        /// </summary>
        public event EventHandler RoleUpdated;

        /// <summary>
        /// Occurs when [delete click].
        /// </summary>
        public event EventHandler DeleteClick;

    }

}