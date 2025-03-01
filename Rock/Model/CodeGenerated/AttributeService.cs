//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Attribute Service class
    /// </summary>
    public partial class AttributeService : Service<Attribute>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public AttributeService(RockContext context) : base(context)
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Attribute item, out string errorMessage )
        {
            errorMessage = string.Empty;
 
            if ( new Service<RegistrationTemplateFormField>( Context ).Queryable().Any( a => a.AttributeId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", Attribute.FriendlyTypeName, RegistrationTemplateFormField.FriendlyTypeName );
                return false;
            }  
            return true;
        }
    }

    /// <summary>
    /// Generated Extension Methods
    /// </summary>
    public static partial class AttributeExtensionMethods
    {
        /// <summary>
        /// Clones this Attribute object to a new Attribute object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static Attribute Clone( this Attribute source, bool deepCopy )
        {
            if (deepCopy)
            {
                return source.Clone() as Attribute;
            }
            else
            {
                var target = new Attribute();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another Attribute object to this Attribute object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this Attribute target, Attribute source )
        {
            target.Id = source.Id;
            target.AllowSearch = source.AllowSearch;
            target.DefaultValue = source.DefaultValue;
            target.Description = source.Description;
            target.EnableHistory = source.EnableHistory;
            target.EntityTypeId = source.EntityTypeId;
            target.EntityTypeQualifierColumn = source.EntityTypeQualifierColumn;
            target.EntityTypeQualifierValue = source.EntityTypeQualifierValue;
            target.FieldTypeId = source.FieldTypeId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.IconCssClass = source.IconCssClass;
            target.IsActive = source.IsActive;
            target.IsAnalytic = source.IsAnalytic;
            target.IsAnalyticHistory = source.IsAnalyticHistory;
            target.IsGridColumn = source.IsGridColumn;
            target.IsIndexEnabled = source.IsIndexEnabled;
            target.IsMultiValue = source.IsMultiValue;
            target.IsRequired = source.IsRequired;
            target.IsSystem = source.IsSystem;
            target.Key = source.Key;
            target.Name = source.Name;
            target.Order = source.Order;
            target.PostHtml = source.PostHtml;
            target.PreHtml = source.PreHtml;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
