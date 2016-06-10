﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using wallabag.Api.Models;
using wallabag.Api.Responses;

namespace wallabag.Api
{
    /// <summary>
    /// Represents an instance of WallabagClient which is used to access the API.
    /// </summary>
    public partial class WallabagClient
    {
        /// <summary>
        /// Returns a list of items filtered by given parameters.
        /// </summary>
        /// <param name="IsRead">Indicates if the item is read (archived) or not.</param>
        /// <param name="IsStarred">Indicates if the item is starred.</param>
        /// <param name="DateOrder">Sort order, in which the items should be returned. Can be <see cref="WallabagDateOrder.ByCreationDate"/> or <see cref="WallabagDateOrder.ByLastModificationDate"/>.</param>
        /// <param name="SortOrder">"Classic" sort order, ascending or descending.</param>
        /// <param name="PageNumber">Number of page.</param>
        /// <param name="ItemsPerPage">Number of items per page.</param>
        /// <param name="Tags">An array of tags that applies to all items.</param>
        /// <returns></returns>
        public async Task<IEnumerable<WallabagItem>> GetItemsAsync(
            bool? IsRead = null,
            bool? IsStarred = null,
            WallabagDateOrder? DateOrder = null,
            WallabagSortOrder? SortOrder = null,
            int? PageNumber = null,
            int? ItemsPerPage = null,
            IEnumerable<string> Tags = null)
        {
            return (await GetItemsWithEnhancedMetadataAsync(IsRead, IsStarred, DateOrder, SortOrder, PageNumber, ItemsPerPage, Tags))?.Items;
        }

        /// <summary>
        /// Returns a result of <see cref="ItemCollectionResponse"/> that contains metadata (number of pages, current page, etc.) along with the items.
        /// </summary>
        /// <param name="IsRead">Indicates if the item is read (archived) or not.</param>
        /// <param name="IsStarred">Indicates if the item is starred.</param>
        /// <param name="DateOrder">Sort order, in which the items should be returned. Can be <see cref="WallabagDateOrder.ByCreationDate"/> or <see cref="WallabagDateOrder.ByLastModificationDate"/>.</param>
        /// <param name="SortOrder">"Classic" sort order, ascending or descending.</param>
        /// <param name="PageNumber">Number of page.</param>
        /// <param name="ItemsPerPage">Number of items per page.</param>
        /// <param name="Tags">An array of tags that applies to all items.</param>
        /// <returns></returns>
        public async Task<ItemCollectionResponse> GetItemsWithEnhancedMetadataAsync(
            bool? IsRead = null,
            bool? IsStarred = null,
            WallabagDateOrder? DateOrder = null,
            WallabagSortOrder? SortOrder = null,
            int? PageNumber = null,
            int? ItemsPerPage = null,
           IEnumerable<string> Tags = null)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (IsRead != null)
                parameters.Add("archive", ((bool)IsRead).ToInt());
            if (IsStarred != null)
                parameters.Add("starred", ((bool)IsStarred).ToInt());
            if (DateOrder != null)
                parameters.Add("sort", (DateOrder == WallabagDateOrder.ByCreationDate ? "created" : "updated"));
            if (SortOrder != null)
                parameters.Add("order", (SortOrder == WallabagSortOrder.Ascending ? "asc" : "desc"));
            if (PageNumber != null)
                parameters.Add("page", PageNumber);
            if (ItemsPerPage != null)
                parameters.Add("perPage", ItemsPerPage);
            if (Tags != null)
                parameters.Add("tags", System.Net.WebUtility.HtmlEncode(Tags.ToCommaSeparatedString()));

            var jsonString = await ExecuteHttpRequestAsync(HttpRequestMethod.Get, "/entries", parameters);
            var response = await ParseJsonFromStringAsync<ItemCollectionResponse>(jsonString);

            if (response != null)
                foreach (var item in response.Items)
                    CheckUriOfItem(item);

            return response;
        }

        /// <summary>
        /// Returns an item by the given id.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        /// <returns><see cref="WallabagItem"/></returns>
        public async Task<WallabagItem> GetItemAsync(int itemId)
        {
            var jsonString = await ExecuteHttpRequestAsync(HttpRequestMethod.Get, $"/entries/{itemId}");
            var result = await ParseJsonFromStringAsync<WallabagItem>(jsonString);
            CheckUriOfItem(result);
            return result;
        }

        private void CheckUriOfItem(WallabagItem item)
        {
            if (item?.PreviewImageUri?.IsAbsoluteUri == false)
            {
                var itemUri = new Uri(item.Url);
                var itemHost = new Uri(itemUri.AbsoluteUri.Replace(itemUri.AbsolutePath, string.Empty));
                item.PreviewImageUri = new Uri(itemHost, item.PreviewImageUri);
            }
        }

        /// <summary>
        /// Represents the order by which the items should be sorted.
        /// </summary>
        public enum WallabagDateOrder
        {
            /// <summary>
            /// Sorts the items by creation date.
            /// </summary>
            ByCreationDate,
            /// <summary>
            /// Sorts the items by last modification date.
            /// </summary>
            ByLastModificationDate
        }

        /// <summary>
        /// Represents the sorting method.
        /// </summary>
        public enum WallabagSortOrder
        {
            /// <summary>
            /// Sorts the items ascending.
            /// </summary>
            Ascending,
            /// <summary>
            /// Sorts the items descending.
            /// </summary>
            Descending
        }
    }
}
