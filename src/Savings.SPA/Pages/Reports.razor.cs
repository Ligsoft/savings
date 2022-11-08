﻿using Microsoft.AspNetCore.Components;
using Savings.Model;
using Savings.SPA.Services;

namespace Savings.SPA.Pages
{
    public partial class Reports : ComponentBase
    {

        [Inject]
        private ISavingsApi savingsAPI { get; set; }

        private RecurrentMoneyItem[] RecurrentItems { get; set; }

        private MaterializedMoneyItem[] Projections { get; set; }

        public string FilterCategoryGroupByPeriod { get; set; } = "yy/MM";

        public int FilterCategoryLastMonths { get; set; } = 12;

        async Task FilterCategoryLastMonthsOnChange(ChangeEventArgs e)
        {
            FilterCategoryLastMonths = int.Parse(e.Value.ToString());

            await InitializeCategoryResume();
            StateHasChanged();
        }

        async Task FilterGroupByCategoryPeriodChanged(ChangeEventArgs e)
        {
            var selectedString = e.Value.ToString();
            FilterCategoryGroupByPeriod = string.IsNullOrWhiteSpace(selectedString) ? null : selectedString;

            await InitializeCategoryResume();
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            await InitializeInstallmentResume();
            await InitializeCategoryResume();
        }

        async Task InitializeInstallmentResume()
        {
            var recurrentItems = await savingsAPI.GetRecurrentMoneyItems(null, true, null, null);
            RecurrentItems = recurrentItems.Where(x => x.EndDate.HasValue && x.EndDate.Value >= DateTime.Now && x.Type == MoneyType.InstallmentPayment).OrderBy(x => x.Note).ToArray();
            DateTime endDate = DateTime.Now.AddMonths(1);
            if (RecurrentItems.Any())
            {
                endDate = RecurrentItems.Max(x => x.EndDate.Value);
            }
            var projections = await savingsAPI.GetSavings(null, endDate, true);
            Projections = projections.Where(x => x.RecurrentMoneyItemID.HasValue && x.Amount != 0 && x.Date >= DateTime.Now).ToArray();
        }


        async Task InitializeCategoryResume()
        {
            statistics = await savingsAPI.GetCategoryResume(FilterCategoryGroupByPeriod, FilterCategoryLastMonths);
        }



        string FormatAmount(object value)
        {
            return ((double)value).ToString("N2");
        }

        ReportCategoryData[] statistics;
    }

}