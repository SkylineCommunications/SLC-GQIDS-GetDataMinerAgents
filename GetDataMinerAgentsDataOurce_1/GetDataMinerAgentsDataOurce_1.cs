/*
****************************************************************************
*  Copyright (c) 2024,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

21/03/2024	1.0.0.1		Matthias Declerck, Skyline	Initial version
****************************************************************************
*/

namespace GetDataMinerAgentsDataSource_1
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Exceptions;
    using Skyline.DataMiner.Net.Messages;

    [GQIMetaData(Name = "Agents")]
    public class GetAgentsDataSource : IGQIDataSource, IGQIOnInit
    {
        private GQIDMS _dms;

        private readonly GQIColumn[] _columns = new GQIColumn[]
        {
            new GQIStringColumn("Agent ID"),
            new GQIStringColumn("Agent Name"),
            new GQIStringColumn("DMA GUID"),
            new GQIStringColumn("Agent State"),
        };

        public OnInitOutputArgs OnInit(OnInitInputArgs args)
        {
            if (args?.DMS == null)
                throw new ArgumentNullException($"{nameof(OnInitInputArgs)} or {nameof(GQIDMS)} is null.");

            _dms = args.DMS;
            return default;
        }

        public GQIColumn[] GetColumns()
        {
            return _columns;
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            if (_dms == null)
                throw new ArgumentNullException($"{nameof(GQIDMS)} is null.");

            DMSMessage[] resp = null;
            try
            {
                var req = new GetInfoMessage(InfoType.DataMinerInfo);
                resp = _dms.SendMessages(req);
            }
            catch (Exception ex)
            {
                throw new DataMinerSecurityException($"Issue occurred in {nameof(GetAgentsDataSource)} when sending request {nameof(GetInfoMessage)}.{InfoType.DataMinerInfo}: {ex}", ex);
            }

            if (resp == null || resp.Length == 0)
                throw new Exception($"Response is null or empty");

            var dmaResponses = resp.OfType<GetDataMinerInfoResponseMessage>().ToArray();
            if (dmaResponses.Length == 0)
                throw new Exception($"{nameof(dmaResponses)} is empty");

            var rows = new List<GQIRow>(dmaResponses.Length);
            foreach (var agent in dmaResponses)
            {
                rows.Add(new GQIRow(new GQICell[]
                {
                    new GQICell() { Value = agent.ID, DisplayValue = agent.ID.ToString(CultureInfo.InvariantCulture) },
                    new GQICell() { Value = agent.AgentName, DisplayValue = agent.AgentName },
                    new GQICell() { Value = agent.DataMinerGuid, DisplayValue = agent.DataMinerGuid.ToString() },
                    new GQICell() { Value = agent.ConnectionState, DisplayValue = agent.ConnectionState.ToString() },
                }));
            }

            return new GQIPage(rows.ToArray())
            {
                HasNextPage = false,
            };
        }
    }
}