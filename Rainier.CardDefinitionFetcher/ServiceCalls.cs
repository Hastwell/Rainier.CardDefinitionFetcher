/*************************************************************************
* Rainier Card Definition Fetcher
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using ClientNetworking;
using ClientNetworking.Models.Commerce;
using ClientNetworking.Models.Query;
using ClientNetworking.Util;
using SharedLogicUtils.DataTypes.Quest;
using SharedLogicUtils.Services.Query.Contexts;
using SharedLogicUtils.Services.Query.Responses;
using SharedLogicUtils.source.Services.Query.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.Tools.RainierCardDefinitionFetcher
{
    internal static class ServiceCalls
    {
        internal static PurchaseShopOfferingsResponse PurchaseItem(Client client, string itemToBuy)
        {
            Guid idempotencyKey = Guid.NewGuid();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\tIdempotency for this transaction is {idempotencyKey}");
            Console.ResetColor();

            List<PurchaseRequestDatum> requestDatum = new()
            {
                new PurchaseRequestDatum(itemToBuy, 1)
            };

            RainierContext rc = new RainierContext { actionGroupType = SharedLogicUtils.source.DataTypes.Analytics.ActionGroupType.Shop };
            PurchaseShopOfferingsResponse response = client.MakeSyncCall<List<PurchaseRequestDatum>, string, RainierContext, PurchaseShopOfferingsResponse>(client.PurchaseShopOfferingsAsync<RainierContext>, requestDatum, idempotencyKey.ToString(), rc);
            return response;
        }

        internal static GetWalletResponse GetWallet(Client client)
        {
            GetWalletResponse wallet = client.MakeSyncCall<GetWalletResponse>(client.GetWalletAsync);
            return wallet;
        }
    }
}
