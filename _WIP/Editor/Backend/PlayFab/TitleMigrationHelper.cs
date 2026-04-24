#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="TitleMigrationHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using global::PlayFab.AdminModels;

    public static class TitleMigrationHelper
    {
        public static void MigrateTitleData(string sourceTitleId, string sourceSecretKey, string destTitleId, string destSecretKey, List<string> titleDataKeys)
        {
            // Migrating all TitleData keys we care about from Dev to Prod
            foreach (var key in titleDataKeys)
            {
                var getTitleDataResult = PlayFabEditorUtil.GetTitleData(sourceTitleId, sourceSecretKey, key);
                var json = getTitleDataResult.Data[key];

                PlayFabEditorUtil.SetTitleData(destTitleId, destSecretKey, key, json);
            }
        }

        public static void MigratCurrencies(string sourceTitleId, string sourceSecretKey, string destTitleId, string destSecretKey)
        {
            var currencies = PlayFabEditorUtil.GetCurrencies(sourceTitleId, sourceSecretKey);
            PlayFabEditorUtil.AddCurrencies(destTitleId, destSecretKey, currencies.VirtualCurrencies);
        }

        private static void MigrateCatalog(
            string sourceTitleId,
            string sourceSecretKey,
            string destTitleId,
            string destSecretKey,
            string catalogVersion,
            bool isDefaultCatalog,
            List<string> stores)
        {
            // Migrating Drop Tables and CatalogItems from Dev to Prod (Note that this system does not remove any items that no longer exist)
            var dropTablesResult = PlayFabEditorUtil.GetDropTables(sourceTitleId, sourceSecretKey, catalogVersion);
            var catalogResult = PlayFabEditorUtil.GetCatalogItems(sourceTitleId, sourceSecretKey, catalogVersion);
            var dropTables = dropTablesResult.Tables.Values.Select(x => new RandomResultTable { Nodes = x.Nodes, TableId = x.TableId }).ToList();
            var catalogItems = catalogResult.Catalog;

            while (dropTables.Count > 0 && catalogItems.Count > 0)
            {
                List<CatalogItem> catalogItemsToUpload = new List<CatalogItem>();
                List<RandomResultTable> dropTablesToUpload = new List<RandomResultTable>();

                // Catalog Items
                foreach (var item in catalogItems)
                {
                    if (CatalogItemRefererncesDropTable(item, dropTables) == false)
                    {
                        catalogItemsToUpload.Add(item);
                    }
                }

                PlayFabEditorUtil.UpdateCatalogItems(destTitleId, destSecretKey, catalogVersion, catalogItemsToUpload, isDefaultCatalog);
                catalogItems.RemoveAll(x => catalogItemsToUpload.Contains(x));

                // Drop Tables
                foreach (var dropTable in dropTables)
                {
                    if (DropTableReferencesCatalogItem(dropTable, catalogItems) == false)
                    {
                        dropTablesToUpload.Add(dropTable);
                    }
                }

                PlayFabEditorUtil.UpdateDropTables(destTitleId, destSecretKey, catalogVersion, dropTablesToUpload);
                dropTables.RemoveAll(x => dropTablesToUpload.Contains(x));
            }

            // Migrating Stores
            foreach (var storeId in stores)
            {
                var storeResult = PlayFabEditorUtil.GetStoreItems(sourceTitleId, sourceSecretKey, catalogVersion, storeId);
                PlayFabEditorUtil.UpdateStoreItems(destTitleId, destSecretKey, catalogVersion, storeId, storeResult.Store);
            }
        }

        private static bool CatalogItemRefererncesDropTable(CatalogItem catalogItem, List<RandomResultTable> dropTables)
        {
            var catalogItemDropTables = catalogItem?.Bundle?.BundledResultTables;

            if (catalogItemDropTables != null)
            {
                foreach (var catalogItemDropTable in catalogItemDropTables)
                {
                    foreach (var dropTable in dropTables)
                    {
                        if (dropTable.TableId == catalogItemDropTable)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool DropTableReferencesCatalogItem(RandomResultTable dropTable, List<CatalogItem> catalogItems)
        {
            var dropTableCatalogItems = dropTable.Nodes.Where(x => x.ResultItemType == ResultTableNodeType.ItemId).Select(x => x.ResultItem).ToList();

            if (dropTableCatalogItems?.Count > 0)
            {
                foreach (var dropTableCatalogItem in dropTableCatalogItems)
                {
                    foreach (var catalogItem in catalogItems)
                    {
                        if (dropTableCatalogItem == catalogItem.ItemId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}

#endif
