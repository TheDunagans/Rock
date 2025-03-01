﻿﻿// <copyright>
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The person doing the registration. For example, Dad signing his kids up for camp. Dad is the Registration person and the kids would be Registrants
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "Registration" )]
    [DataContract]
    public partial class Registration : Model<Registration>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [Required]
        [DataMember]
        [IgnoreCanDelete]
        public int RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [MaxLength( 100 )]
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [DataMember]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is temporary.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is temporary; otherwise, <c>false</c>.
        /// </value>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Gets or sets the last payment reminder date time.
        /// </summary>
        /// <value>
        /// The last payment reminder date time.
        /// </value>
        [DataMember]
        public DateTime? LastPaymentReminderDateTime
        {
            get
            {
                return _lastPaymentReminderDateTime.HasValue ? _lastPaymentReminderDateTime : this.CreatedDateTime;
            }
            set
            {
                _lastPaymentReminderDateTime = value;
            }
        }
        private DateTime? _lastPaymentReminderDateTime;

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaInclude]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [NotMapped]
        public virtual int? PersonId
        {
            get { return PersonAlias != null ? PersonAlias.PersonId : ( int? ) null; }
        }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrant> Registrants
        {
            get { return _registrants ?? ( _registrants = new Collection<RegistrationRegistrant>() ); }
            set { _registrants = value; }
        }
        private ICollection<RegistrationRegistrant> _registrants;

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalCost
        {
            get
            {
                if ( Registrants != null )
                {
                    return Registrants.Sum( r => r.TotalCost );
                }

                return 0.0M;
            }
        }

        /// <summary>
        /// Gets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal DiscountedCost
        {
            get
            {
                var discountedCost = 0.0m;
                if ( Registrants != null )
                {
                    foreach ( var registrant in Registrants )
                    {
                        discountedCost += registrant.DiscountedCost( DiscountPercentage, DiscountAmount );
                    }
                }
                return discountedCost;
            }
        }
        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <value>
        /// The total paid.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal TotalPaid
        {
            get
            {
                return this.GetTotalPaid();
            }
        }

        /// <summary>
        /// Gets the balance due.
        /// </summary>
        /// <value>
        /// The balance due.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual decimal BalanceDue
        {
            get
            {
                return DiscountedCost - TotalPaid;
            }
        }

        /// <summary>
        /// Gets the registration template identifier.
        /// NOTE: this is needed so that Registration Attributes can have a RegistrationTemplateId qualifier
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual int? RegistrationTemplateId
        {
            get
            {
                if ( this.RegistrationInstance == null )
                {
                    return new RegistrationInstanceService( new RockContext() ).GetSelect( this.RegistrationInstanceId, a => a.RegistrationTemplateId );
                }

                return this.RegistrationInstance.RegistrationTemplateId;
            }
        }

        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <value>
        /// The payments.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual IQueryable<FinancialTransactionDetail> Payments
        {
            get
            {
                return this.GetPayments();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a summary of the registration
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <returns></returns>
        public string GetSummary( RegistrationInstance registrationInstance = null )
        {
            var result = new StringBuilder();
            result.Append( "Event registration payment" );

            var instance = registrationInstance ?? RegistrationInstance;
            if ( instance != null )
            {
                result.AppendFormat( " for {0} [ID:{1}]", instance.Name, instance.Id );
                if ( instance.RegistrationTemplate != null )
                {
                    result.AppendFormat( " (Template: {0} [ID:{1}])", instance.RegistrationTemplate.Name, instance.RegistrationTemplate.Id );
                }
            }

            string registrationPerson = PersonAlias != null && PersonAlias.Person != null ?
                PersonAlias.Person.FullName :
                string.Format( "{0} {1}", FirstName, LastName );
            result.AppendFormat( @".
Registration By: {0} Total Cost/Fees:{1}
", registrationPerson, DiscountedCost.FormatAsCurrency() );

            var registrantPersons = new List<string>();
            if ( Registrants != null )
            {
                foreach ( var registrant in Registrants.Where( r => r.PersonAlias != null && r.PersonAlias.Person != null ) )
                {
                    registrantPersons.Add( string.Format( "{0} Cost/Fees:{1}",
                        registrant.PersonAlias.Person.FullName,
                        registrant.DiscountedCost( DiscountPercentage, DiscountAmount ).FormatAsCurrency() ) );
                }
            }
            result.AppendFormat( "Registrants: {0}", registrantPersons.AsDelimited( ", " ) );

            return result.ToString();
        }

        /// <summary>
        /// Saves the person notes and history.
        /// </summary>
        /// <param name="registrationPerson">The person that created the registration</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="previousRegistrantPersonIds">The person ids that have already registered prior to this registration</param>
        public void SavePersonNotesAndHistory( Person registrationPerson, int? currentPersonAliasId, List<int> previousRegistrantPersonIds )
        {
            SavePersonNotesAndHistory( registrationPerson.FirstName, registrationPerson.LastName, currentPersonAliasId, previousRegistrantPersonIds );
        }

        /// <summary>
        /// Saves the person notes and history.
        /// </summary>
        /// <param name="registrationPersonFirstName">First name of the registration person.</param>
        /// <param name="registrationPersonLastName">Last name of the registration person.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="previousRegistrantPersonIds">The previous registrant person ids.</param>
        public void SavePersonNotesAndHistory( string registrationPersonFirstName, string registrationPersonLastName, int? currentPersonAliasId, List<int> previousRegistrantPersonIds )
        {
            // Setup Note settings
            Registration registration = this;
            NoteTypeCache noteType = null;
            using ( RockContext rockContext = new RockContext() )
            {
                RegistrationInstance registrationInstance = registration.RegistrationInstance ?? new RegistrationInstanceService( rockContext ).Get( registration.RegistrationInstanceId );
                RegistrationTemplate registrationTemplate = registrationInstance.RegistrationTemplate ?? new RegistrationTemplateService( rockContext ).Get( registrationInstance.RegistrationTemplateId );

                if ( registrationTemplate != null && registrationTemplate.AddPersonNote )
                {
                    noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PERSON_EVENT_REGISTRATION.AsGuid() );
                    if ( noteType != null )
                    {
                        var noteService = new NoteService( rockContext );
                        var personAliasService = new PersonAliasService( rockContext );

                        Person registrar = null;
                        if ( registration.PersonAliasId.HasValue )
                        {
                            registrar = personAliasService.GetPerson( registration.PersonAliasId.Value );
                        }

                        var registrantNames = new List<string>();

                        // Get each registrant
                        foreach ( var registrantPersonAliasId in registration.Registrants
                            .Where( r => r.PersonAliasId.HasValue )
                            .Select( r => r.PersonAliasId.Value )
                            .ToList() )
                        {
                            var registrantPerson = personAliasService.GetPerson( registrantPersonAliasId );
                            if ( registrantPerson != null && ( previousRegistrantPersonIds == null || !previousRegistrantPersonIds.Contains( registrantPerson.Id ) ) )
                            {
                                var noteText = new StringBuilder();
                                noteText.AppendFormat( "Registered for {0}", registrationInstance.Name );

                                string registrarFullName = string.Empty;

                                if ( registrar != null && registrar.Id != registrantPerson.Id )
                                {
                                    registrarFullName = string.Format( " by {0}", registrar.FullName );
                                    registrantNames.Add( registrantPerson.FullName );
                                }

                                if ( registrar != null && ( registrationPersonFirstName != registrar.NickName || registrationPersonLastName != registrar.LastName ) )
                                {
                                    registrarFullName = string.Format( " by {0}", registrationPersonFirstName + " " + registrationPersonLastName );
                                }

                                noteText.Append( registrarFullName );

                                if ( noteText.Length > 0 )
                                {
                                    var note = new Note();
                                    note.NoteTypeId = noteType.Id;
                                    note.IsSystem = false;
                                    note.IsAlert = false;
                                    note.IsPrivateNote = false;
                                    note.EntityId = registrantPerson.Id;
                                    note.Caption = string.Empty;
                                    note.Text = noteText.ToString();
                                    if ( registrar == null )
                                    {
                                        note.CreatedByPersonAliasId = currentPersonAliasId;
                                    }
                                    else
                                    {
                                        note.CreatedByPersonAliasId = registrar.PrimaryAliasId;
                                    }
                                    noteService.Add( note );
                                }

                                var changes = new History.HistoryChangeList();
                                changes.AddChange( History.HistoryVerb.Registered, History.HistoryChangeType.Record, null );
                                HistoryService.SaveChanges(
                                    rockContext,
                                    typeof( Person ),
                                    Rock.SystemGuid.Category.HISTORY_PERSON_REGISTRATION.AsGuid(),
                                    registrantPerson.Id,
                                    changes,
                                    registrationInstance.Name,
                                    typeof( Registration ),
                                    registration.Id,
                                    false,
                                    currentPersonAliasId,
                                    rockContext.SourceOfChange );
                            }
                        }

                        if ( registrar != null && registrantNames.Any() )
                        {
                            string namesText = string.Empty;
                            if ( registrantNames.Count >= 2 )
                            {
                                int lessOne = registrantNames.Count - 1;
                                namesText = registrantNames.Take( lessOne ).ToList().AsDelimited( ", " ) +
                                    " and " +
                                    registrantNames.Skip( lessOne ).Take( 1 ).First() + " ";
                            }
                            else
                            {
                                namesText = registrantNames.First() + " ";
                            }

                            var note = new Note();
                            note.NoteTypeId = noteType.Id;
                            note.IsSystem = false;
                            note.IsAlert = false;
                            note.IsPrivateNote = false;
                            note.EntityId = registrar.Id;
                            note.Caption = string.Empty;
                            note.Text = string.Format( "Registered {0} for {1}", namesText, registrationInstance.Name );
                            noteService.Add( note );

                            var changes = new History.HistoryChangeList();
                            changes.AddChange( History.HistoryVerb.Registered, History.HistoryChangeType.Record, namesText );

                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_REGISTRATION.AsGuid(),
                                registrar.Id,
                                changes,
                                registrationInstance.Name,
                                typeof( Registration ),
                                registration.Id,
                                false,
                                currentPersonAliasId,
                                rockContext.SourceOfChange );
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( PersonAlias != null && PersonAlias.Person != null )
            {
                return PersonAlias.Person.FullName;
            }

            string personName = string.Format( "{0} {1}", FirstName, LastName );
            return string.IsNullOrWhiteSpace( personName ) ? "Registration" : personName.Trim();

        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                return RegistrationInstance != null ? RegistrationInstance : base.ParentAuthority;
            }
        }
        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationConfiguration : EntityTypeConfiguration<Registration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationConfiguration"/> class.
        /// </summary>
        public RegistrationConfiguration()
        {
            this.HasRequired( r => r.RegistrationInstance ).WithMany( t => t.Registrations ).HasForeignKey( r => r.RegistrationInstanceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Group ).WithMany().HasForeignKey( r => r.GroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Extension Methods

    /// <summary>
    /// Extension method class for Registration
    /// </summary>
    public static partial class RegistrationExtensionMethods
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<FinancialTransactionDetail> GetPayments( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetPayments( registration != null ? registration.Id : 0 );
        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetTotalPayments( registration != null ? registration.Id : 0 );
        }
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Registration Helper Class
    /// </summary>
    [Serializable]
    public class RegistrationInfo
    {
        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int? RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the slots available.
        /// </summary>
        /// <value>
        /// The slots available.
        /// </value>
        public int? SlotsAvailable { get; set; }

        /// <summary>
        /// Gets or sets your first name.
        /// </summary>
        /// <value>
        /// Your first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets your last name.
        /// </summary>
        /// <value>
        /// Your last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the discount code.
        /// </summary>
        /// <value>
        /// The discount code.
        /// </value>
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the previous payment total.
        /// </summary>
        /// <value>
        /// The previous payment total.
        /// </value>
        public decimal PreviousPaymentTotal { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        /// <value>
        /// The payment amount.
        /// </value>
        public decimal? PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        public List<RegistrantInfo> Registrants { get; set; }

        /// <summary>
        /// Gets the registrant count.
        /// </summary>
        /// <value>
        /// The registrant count.
        /// </value>
        public int RegistrantCount
        {
            get { return Registrants.Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        public RegistrationInfo()
        {
            FamilyGuid = Guid.Empty;
            Registrants = new List<RegistrantInfo>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        public RegistrationInfo( Person person )
            : this()
        {
            if ( person != null )
            {
                FirstName = person.NickName;
                LastName = person.LastName;
                ConfirmationEmail = person.Email;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInfo" /> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        public RegistrationInfo( Registration registration, RockContext rockContext )
            : this()
        {
            if ( registration != null )
            {
                RegistrationId = registration.Id;
                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    FirstName = registration.PersonAlias.Person.NickName;
                    LastName = registration.PersonAlias.Person.LastName;
                    ConfirmationEmail = registration.ConfirmationEmail;
                }
                                
                DiscountCode = registration.DiscountCode != null ? registration.DiscountCode.Trim() : string.Empty;
                DiscountPercentage = registration.DiscountPercentage;
                DiscountAmount = registration.DiscountAmount;
                TotalCost = registration.TotalCost;
                DiscountedCost = registration.DiscountedCost;
                SlotsAvailable = registration.Registrants.Where( r => !r.OnWaitList ).Count();

                if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
                {
                    var family = registration.PersonAlias.Person.GetFamily( rockContext );
                    if ( family != null )
                    {
                        FamilyGuid = family.Guid;
                    }
                }

                foreach ( var registrant in registration.Registrants )
                {
                    Registrants.Add( new RegistrantInfo( registrant, rockContext ) );
                }
            }
        }

        /// <summary>
        /// Gets the options that should be available for additional registrants to specify the family they belong to
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="currentRegistrantIndex">Index of the current registrant.</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetFamilyOptions( RegistrationTemplate template, int currentRegistrantIndex )
        {
            // Return a dictionary of family group guid, and the formated name (i.e. "Ted & Cindy Decker" )
            var result = new Dictionary<Guid, string>();

            // Get all the registrants prior to the current registrant
            var familyRegistrants = new Dictionary<Guid, List<RegistrantInfo>>();
            for ( int i = 0; i < currentRegistrantIndex; i++ )
            {
                if ( Registrants != null && Registrants.Count > i )
                {
                    var registrant = Registrants[i];
                    familyRegistrants.AddOrIgnore( registrant.FamilyGuid, new List<RegistrantInfo>() );
                    familyRegistrants[registrant.FamilyGuid].Add( registrant );
                }
                else
                {
                    break;
                }
            }

            // Loop through those registrants
            foreach ( var keyVal in familyRegistrants )
            {
                // Find all the people and group them by same last name
                var lastNames = new Dictionary<string, List<string>>();
                foreach ( var registrant in keyVal.Value )
                {
                    string firstName = registrant.GetFirstName( template );
                    string lastName = registrant.GetLastName( template );
                    lastNames.AddOrIgnore( lastName, new List<string>() );
                    lastNames[lastName].Add( firstName );
                }

                // Build a formated output for each unique last name
                var familyNames = new List<string>();
                foreach ( var lastName in lastNames )
                {
                    familyNames.Add( string.Format( "{0} {1}", lastName.Value.AsDelimited( " & " ), lastName.Key ) );
                }

                // Join each of the formated values for each unique last name for the current family
                result.Add( keyVal.Key, familyNames.AsDelimited( " and " ) );
            }

            return result;
        }

    }

    /// <summary>
    /// Registrant Helper Class
    /// </summary>
    [Serializable]
    public class RegistrantInfo
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        public int RegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the group member identifier.
        /// </summary>
        /// <value>
        /// The group member identifier.
        /// </value>
        public int? GroupMemberId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        /// <value>
        /// The name of the group.
        /// </value>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the person alias unique identifier.
        /// </summary>
        /// <value>
        /// The person alias unique identifier.
        /// </value>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [on wait list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [on wait list]; otherwise, <c>false</c>.
        /// </value>
        public bool OnWaitList { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [discount applies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [discount applies]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountApplies
        {
            get { return _discountApplies; }
            set { _discountApplies = value; }
        }
        private bool _discountApplies = true;

        /// <summary>
        /// Gets the cost with fees.
        /// </summary>
        /// <value>
        /// The cost with fees.
        /// </value>
        public virtual decimal TotalCost
        {
            get
            {
                if ( OnWaitList )
                {
                    return 0.0M;
                }

                var cost = Cost;
                if ( FeeValues != null )
                {
                    cost += FeeValues
                        .Where( f => f.Value != null )
                        .SelectMany( f => f.Value )
                        .Sum( f => f.TotalCost );
                }
                return cost;
            }
        }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public Dictionary<int, FieldValueObject> FieldValues { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of FeeValues (key is RegistrationTemplateFeeId)
        /// </summary>
        /// <value>
        /// The fee values.
        /// </value>
        public Dictionary<int, List<FeeInfo>> FeeValues { get; set; }

        /// <summary>
        /// Gets or sets the signature document Id.
        /// </summary>
        /// <value>
        /// The signature document Id.
        /// </value>
        public int? SignatureDocumentId { get; set; }

        /// <summary>
        /// Gets or sets the signature document key.
        /// </summary>
        /// <value>
        /// The signature document key.
        /// </value>
        public string SignatureDocumentKey { get; set; }

        /// <summary>
        /// Gets or sets the signature document last sent.
        /// </summary>
        /// <value>
        /// The signature document last sent.
        /// </value>
        public DateTime? SignatureDocumentLastSent { get; set; }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedCost( decimal discountPercent, decimal discountAmount )
        {
            if ( OnWaitList )
            {
                return 0.0M;
            }

            var discountedCost = Cost - ( DiscountApplies ? ( Cost * discountPercent ) : 0.0M );
            discountedCost = discountedCost - ( DiscountApplies ? discountAmount : 0.0M );

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <param name="discountAmount">The discount amount.</param>
        /// <returns></returns>
        public virtual decimal DiscountedTotalCost( decimal discountPercent, decimal discountAmount )
        {
            if ( OnWaitList )
            {
                return 0.0M;
            }

            var discountedCost = Cost - ( DiscountApplies ? ( Cost * discountPercent ) : 0.0M );
            if ( FeeValues != null )
            {
                foreach ( var fee in FeeValues.SelectMany( f => f.Value ) )
                {
                    discountedCost += DiscountApplies ? fee.DiscountedCost( discountPercent ) : fee.TotalCost;
                }
            }
            discountedCost = discountedCost - ( DiscountApplies ? discountAmount : 0.0M );

            return discountedCost > 0.0m ? discountedCost : 0.0m;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo"/> class.
        /// </summary>
        public RegistrantInfo()
        {
            Guid = Guid.NewGuid();
            PersonAliasGuid = Guid.Empty;
            PersonId = null;
            GroupMemberId = null;
            GroupName = string.Empty;
            FamilyGuid = Guid.Empty;
            FieldValues = new Dictionary<int, FieldValueObject>();
            FeeValues = new Dictionary<int, List<FeeInfo>>();
            OnWaitList = false;
            DiscountApplies = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo" /> class.
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="person">The person.</param>
        public RegistrantInfo( RegistrationInstance registrationInstance, Person person )
            : this()
        {
            if ( person != null )
            {
                PersonId = person.Id;

                using ( var rockContext = new RockContext() )
                {
                    PersonName = person.FullName;
                    var family = person.GetFamily( rockContext );

                    if ( registrationInstance != null &&
                        registrationInstance.RegistrationTemplate != null )
                    {
                        var templateFields = registrationInstance.RegistrationTemplate.Forms
                            .SelectMany( f => f.Fields )
                            .Where( f => !f.IsInternal && f.ShowCurrentValue );

                        foreach ( var field in templateFields )
                        {
                            object dbValue = GetRegistrantValue( null, person, family, field, rockContext );
                            if ( dbValue != null )
                            {
                                FieldValues.Add( field.Id, new FieldValueObject( field, dbValue ) );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantInfo" /> class.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="rockContext">The rock context.</param>
        public RegistrantInfo( RegistrationRegistrant registrant, RockContext rockContext )
            : this()
        {
            if ( registrant != null )
            {
                Id = registrant.Id;
                Guid = registrant.Guid;
                GroupMemberId = registrant.GroupMemberId;
                GroupName = registrant.GroupMember != null && registrant.GroupMember.Group != null ?
                    registrant.GroupMember.Group.Name : string.Empty;
                RegistrationId = registrant.RegistrationId;
                Cost = registrant.Cost;
                DiscountApplies = registrant.DiscountApplies;
                OnWaitList = registrant.OnWaitList;

                Person person = null;
                Group family = null;

                if ( registrant.PersonAlias != null )
                {
                    PersonId = registrant.PersonAlias.PersonId;
                    PersonAliasGuid = registrant.PersonAlias.Guid;
                    person = registrant.PersonAlias.Person;

                    if ( person != null )
                    {
                        PersonName = person.FullName;
                        family = person.GetFamily( rockContext );
                        if ( family != null )
                        {
                            FamilyGuid = family.Guid;
                        }
                    }
                }

                if ( registrant.Registration != null &&
                    registrant.Registration.RegistrationInstance != null &&
                    registrant.Registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var templateFields = registrant.Registration.RegistrationInstance.RegistrationTemplate.Forms
                        .SelectMany( f => f.Fields );
                    foreach ( var field in templateFields )
                    {
                        object dbValue = GetRegistrantValue( registrant, person, family, field, rockContext );
                        if ( dbValue != null )
                        {
                            FieldValues.Add( field.Id, new FieldValueObject( field, dbValue ) );
                        }
                    }

                    foreach ( var fee in registrant.Fees )
                    {
                        FeeValues.AddOrIgnore( fee.RegistrationTemplateFeeId, new List<FeeInfo>() );
                        FeeValues[fee.RegistrationTemplateFeeId].Add( new FeeInfo( fee ) );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the existing value for a specific field for the given registrant.
        /// </summary>
        /// <param name="registrant">The registrant.</param>
        /// <param name="person">The person.</param>
        /// <param name="family">The family.</param>
        /// <param name="field">The field.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public object GetRegistrantValue( RegistrationRegistrant registrant, Person person, Group family,
            RegistrationTemplateFormField field, RockContext rockContext )
        {
            if ( field.FieldSource == RegistrationFieldSource.PersonField )
            {
                if ( person != null )
                {
                    DefinedValueCache dvPhone = null;

                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.FirstName: return person.NickName;
                        case RegistrationPersonFieldType.MiddleName: return person.MiddleName;
                        case RegistrationPersonFieldType.LastName: return person.LastName;
                        case RegistrationPersonFieldType.Campus:
                            {
                                if ( family != null )
                                {
                                    return family.CampusId;
                                }
                                break;
                            }
                        case RegistrationPersonFieldType.Address:
                            {
                                var location = person.GetHomeLocation( rockContext );
                                if ( location != null )
                                {
                                    return location.Clone();
                                }
                                break;
                            }
                        case RegistrationPersonFieldType.Email: return person.Email;
                        case RegistrationPersonFieldType.Birthdate: return person.BirthDate;
                        case RegistrationPersonFieldType.Grade: return person.GraduationYear;
                        case RegistrationPersonFieldType.Gender: return person.Gender;
                        case RegistrationPersonFieldType.MaritalStatus: return person.MaritalStatusValueId;
                        case RegistrationPersonFieldType.AnniversaryDate: return person.AnniversaryDate;
                        case RegistrationPersonFieldType.MobilePhone:
                            {
                                dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                                break;
                            }
                        case RegistrationPersonFieldType.HomePhone:
                            {
                                dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                                break;
                            }
                        case RegistrationPersonFieldType.WorkPhone:
                            {
                                dvPhone = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                                break;
                            }
                        case RegistrationPersonFieldType.ConnectionStatus: return person.ConnectionStatusValueId;
                    }

                    if ( dvPhone != null )
                    {
                        var phoneNumber = new PersonService( rockContext ).GetPhoneNumber( person, dvPhone );
                        if ( phoneNumber != null )
                        {
                            return phoneNumber.Clone();
                        }
                    }
                }
            }
            else
            {
                var attribute = AttributeCache.Get( field.AttributeId ?? 0 );
                if ( attribute != null )
                {
                    switch ( field.FieldSource )
                    {
                        case RegistrationFieldSource.PersonAttribute:
                            {
                                if ( person != null )
                                {
                                    if ( person.Attributes == null )
                                    {
                                        person.LoadAttributes();
                                    }
                                    return person.GetAttributeValue( attribute.Key );
                                }
                                break;
                            }

                        case RegistrationFieldSource.GroupMemberAttribute:
                            {
                                if ( registrant != null && registrant.GroupMember != null )
                                {
                                    if ( registrant.GroupMember.Attributes == null )
                                    {
                                        registrant.GroupMember.LoadAttributes();
                                    }
                                    return registrant.GroupMember.GetAttributeValue( attribute.Key );
                                }
                                break;
                            }

                        case RegistrationFieldSource.RegistrantAttribute:
                            {
                                if ( registrant != null )
                                {
                                    if ( registrant.Attributes == null )
                                    {
                                        registrant.LoadAttributes();
                                    }
                                    return registrant.GetAttributeValue( attribute.Key );
                                }
                                break;
                            }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetFirstName( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.FirstName );
            if ( value == null )
            {
                // if FirstName isn't prompted for in a registration form, and using an existing Person, get the person's FirstName/NickName from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.NickName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetLastName( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.LastName );
            if ( value == null )
            {
                // if LastName isn't prompted for in a registration form, and using an existing Person, get the person's lastname from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.LastName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns></returns>
        public string GetEmail( RegistrationTemplate template )
        {
            object value = GetPersonFieldValue( template, RegistrationPersonFieldType.Email );
            if ( value == null )
            {
                // if Email isn't prompted for in a registration form, and using an existing Person, get the person's email from the database
                if ( this.PersonId.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( this.PersonId.Value, s => s.Email ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        public object GetPersonFieldValue( RegistrationTemplate template, RegistrationPersonFieldType personFieldType )
        {
            if ( template != null && template.Forms != null )
            {
                var fieldId = template.Forms
                    .SelectMany( t => t.Fields
                        .Where( f =>
                            f.FieldSource == RegistrationFieldSource.PersonField &&
                            f.PersonFieldType == personFieldType )
                        .Select( f => f.Id ) )
                    .FirstOrDefault();

                return FieldValues.ContainsKey( fieldId ) ? FieldValues[fieldId].FieldValue : null;
            }

            return null;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Newtonsoft.Json.JsonConverter( typeof( FieldValueConverter ) )]
    public class FieldValueObject
    {
        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public RegistrationFieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the field value.
        /// </summary>
        /// <value>
        /// The field value.
        /// </value>
        public object FieldValue { get; set; }

        /// <summary>
        /// Gets the type of the field value.
        /// </summary>
        /// <value>
        /// The type of the field value.
        /// </value>
        public Type FieldValueType
        {
            get
            {
                Type valueType = typeof( string );
                if ( FieldSource == RegistrationFieldSource.PersonField )
                {
                    switch ( PersonFieldType )
                    {
                        case RegistrationPersonFieldType.Campus:
                        case RegistrationPersonFieldType.MaritalStatus:
                        case RegistrationPersonFieldType.Grade:
                        case RegistrationPersonFieldType.ConnectionStatus:
                                return typeof( int? );

                        case RegistrationPersonFieldType.Address:
                                return typeof( Location );

                        case RegistrationPersonFieldType.Birthdate:
                                return typeof( DateTime? );

                        case RegistrationPersonFieldType.Gender:
                                return  typeof( Gender );

                        case RegistrationPersonFieldType.MobilePhone:
                        case RegistrationPersonFieldType.HomePhone:
                        case RegistrationPersonFieldType.WorkPhone:
                                return typeof( PhoneNumber );
                    }
                }
                return typeof( string );
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueObject"/> class.
        /// </summary>
        public FieldValueObject()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldValueObject"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="fieldValue">The field value.</param>
        public FieldValueObject( RegistrationTemplateFormField field, object fieldValue)
        {
            FieldSource = field.FieldSource;
            PersonFieldType = field.PersonFieldType;
            FieldValue = fieldValue;
        }

        
    }

    /// <summary>
    /// Registrant  Fee Helper Class
    /// </summary>
    [Serializable]
    public class FeeInfo
    {
        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        /// <value>
        /// The option.
        /// </value>
        [RockObsolete( "1.9" )]
        [Obsolete( "Use RegistrationTemplateFeeItemId + FeeLabel instead" )]
        public string Option
        {
            get => this.FeeLabel;
            set => this.FeeLabel = value;
        }

        /// <summary>
        /// Gets or sets the fee label.
        /// </summary>
        /// <value>
        /// The fee label.
        /// </value>
        public string FeeLabel { get; set; }

        /// <summary>
        /// Gets or sets the registration template fee item identifier.
        /// </summary>
        /// <value>
        /// The registration template fee item identifier.
        /// </value>
        public int RegistrationTemplateFeeItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [discount applies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [discount applies]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountApplies { get; set; }

        /// <summary>
        /// Gets the total cost.
        /// </summary>
        /// <value>
        /// The total cost.
        /// </value>
        public decimal TotalCost
        {
            get { return Cost * Quantity; }
        }

        /// <summary>
        /// Gets or sets the previous cost.
        /// </summary>
        /// <value>
        /// The previous cost.
        /// </value>
        public decimal PreviousCost { get; set; }

        /// <summary>
        /// Discounts the cost.
        /// </summary>
        /// <param name="discountPercent">The discount percent.</param>
        /// <returns></returns>
        public decimal DiscountedCost( decimal discountPercent )
        {
            var discountedCost = TotalCost;

            if ( DiscountApplies )
            {
                discountedCost = discountedCost - ( discountedCost * discountPercent );
            }

            return discountedCost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        public FeeInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="cost">The cost.</param>
        [RockObsolete( "1.9" )]
        [Obsolete( "Use other FeeInfo constructor" )]
        public FeeInfo( string option, int quantity, decimal cost )
            : this()
        {
            Option = option;
            Quantity = quantity;
            Cost = cost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo" /> class.
        /// </summary>
        /// <param name="feeItem">The fee item.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="cost">The cost.</param>
        public FeeInfo( RegistrationTemplateFeeItem feeItem, int quantity, decimal cost )
            : this()
        {
            FeeLabel = feeItem.Name;
            RegistrationTemplateFeeItemId = feeItem.Id;
            Quantity = quantity;
            Cost = cost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeeInfo"/> class.
        /// </summary>
        /// <param name="fee">The fee.</param>
        public FeeInfo( RegistrationRegistrantFee fee )
            : this()
        {
            FeeLabel = fee.RegistrationTemplateFeeItem.Name;
            RegistrationTemplateFeeItemId = fee.RegistrationTemplateFeeItemId.Value;
            Quantity = fee.Quantity;
            Cost = fee.Cost;
            PreviousCost = fee.Cost;
            DiscountApplies = fee.RegistrationTemplateFee != null && fee.RegistrationTemplateFee.DiscountApplies;
        }

    }

    /// <summary>
    /// Helper class for creating summary of cost/fee information to bind to summary grid
    /// </summary>
    public class RegistrationCostSummaryInfo
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public RegistrationCostSummaryType Type { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the discounted cost.
        /// </summary>
        /// <value>
        /// The discounted cost.
        /// </value>
        public decimal DiscountedCost { get; set; }

        /// <summary>
        /// Gets or sets the minimum payment.
        /// </summary>
        /// <value>
        /// The minimum payment.
        /// </value>
        public decimal MinPayment { get; set; }

        /// <summary>
        /// Gets or sets the default payment.
        /// </summary>
        /// <value>
        /// The default payment.
        /// </value>
        public decimal? DefaultPayment { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FieldValueConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            return objectType.IsAssignableFrom( typeof( string ) );
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if ( reader.TokenType == JsonToken.Null )
            {
                return null;
            }

            FieldValueObject fieldValueObject = new FieldValueObject();

            try
            {
                reader.Read();
                while ( reader.TokenType == JsonToken.PropertyName )
                {
                    string str = reader.Value.ToString();
                    if ( string.Equals( str, "FieldSource", StringComparison.OrdinalIgnoreCase ) )
                    {
                        reader.Read();
                        fieldValueObject.FieldSource = (RegistrationFieldSource)serializer.Deserialize( reader, typeof( RegistrationFieldSource ) );
                    }
                    else if ( string.Equals( str, "PersonFieldType", StringComparison.OrdinalIgnoreCase ) )
                    {
                        reader.Read();
                        fieldValueObject.PersonFieldType = (RegistrationPersonFieldType)serializer.Deserialize( reader, typeof( RegistrationPersonFieldType ) );
                    }
                    else if ( string.Equals( str, "FieldValue", StringComparison.OrdinalIgnoreCase ) )
                    {
                        reader.Read();
                        fieldValueObject.FieldValue = serializer.Deserialize( reader, fieldValueObject.FieldValueType );
                    }
                    reader.Read();
                }
            }
            catch { }

            return fieldValueObject;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var fieldValueObject = value as FieldValueObject;
            if ( fieldValueObject != null )
            {
                DefaultContractResolver contractResolver = serializer.ContractResolver as DefaultContractResolver;
                writer.WriteStartObject();
                
                writer.WritePropertyName( ( contractResolver != null ? contractResolver.GetResolvedPropertyName( "FieldSource" ) : "FieldSource" ) );
                serializer.Serialize( writer, fieldValueObject.FieldSource );

                writer.WritePropertyName( ( contractResolver != null ? contractResolver.GetResolvedPropertyName( "PersonFieldType" ) : "PersonFieldType" ) );
                serializer.Serialize( writer, fieldValueObject.PersonFieldType );

                writer.WritePropertyName( ( contractResolver != null ? contractResolver.GetResolvedPropertyName( "FieldValue" ) : "FieldValue" ) );
                serializer.Serialize( writer, fieldValueObject.FieldValue, fieldValueObject.FieldValueType );

                writer.WriteEndObject();
            }
            else
            {
                serializer.Serialize( writer, value );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RegistrationCostSummaryType
    {
        /// <summary>
        /// a cost
        /// 
        /// </summary>
        Cost,

        /// <summary>
        /// a fee
        /// </summary>
        Fee,

        /// <summary>
        /// The discount
        /// </summary>
        Discount,

        /// <summary>
        /// The total
        /// </summary>
        Total
    }

    #endregion

}
