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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for selecting radio button options from an enum.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class EnumFieldAttribute : SelectFieldAttribute
    {
        private const string VALUES = "values";
        private const string FIELDTYPE = "fieldtype";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="enumSourceType">Type of the enum source.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.  If multiple values are supported (i.e. checkbox) each value should be delimited by a comma</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public EnumFieldAttribute( string name, string description, Type enumSourceType, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectSingleFieldType ).FullName )
        {
            var list = new List<string>();
            foreach ( var value in Enum.GetValues( enumSourceType ) )
            {
                list.Add( string.Format( "{0}^{1}", (int)value, value.ToString().SplitCase() ) );
            }
            
            var listSource = string.Join( ",", list );
            FieldConfigurationValues.Add( VALUES, new Field.ConfigurationValue( listSource ) );
            FieldConfigurationValues.Add( FIELDTYPE, new Field.ConfigurationValue( "rb" ) );
        }
    }
}