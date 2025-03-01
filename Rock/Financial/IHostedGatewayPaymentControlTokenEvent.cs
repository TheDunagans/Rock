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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Financial
{
    /// <summary>
    /// HostedGatewayPaymentControls that implement this have a TokenReceived event that can be used to notify that a payment token response has been received
    /// </summary>
    public interface IHostedGatewayPaymentControlTokenEvent
    {
        /// <summary>
        /// Occurs when a payment token is received from the hosted gateway
        /// </summary>
        event EventHandler TokenReceived;
    }
}
