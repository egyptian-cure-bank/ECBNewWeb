﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ECBNewWeb.DataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MarketEntities : DbContext
    {
        public MarketEntities()
            : base("name=MarketEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<doner> doners { get; set; }
        public DbSet<market> markets { get; set; }
        public DbSet<center> centers { get; set; }
        public DbSet<government> governments { get; set; }
        public DbSet<marketingrectype> marketingrectypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<KnowingMethod> KnowingMethods { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<BookResposibility> BookResposibilities { get; set; }
        public DbSet<CanceledReceipt> CanceledReceipts { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DonationFrequency> DonationFrequencies { get; set; }
        public DbSet<DonorOf> DonorOfs { get; set; }
        public DbSet<UserSite> UserSites { get; set; }
        public DbSet<TypeContact> TypeContacts { get; set; }
        public DbSet<MarketingLicens> MarketingLicenses { get; set; }
        public DbSet<CurrencyCovnersionRate> CurrencyCovnersionRates { get; set; }
        public DbSet<BookType> BookTypes { get; set; }
        public DbSet<marketingsite> marketingsites { get; set; }
        public DbSet<DonationPurpose> DonationPurposes { get; set; }
        public DbSet<ChequeBank> ChequeBanks { get; set; }
        public DbSet<BookRequestDetail> BookRequestDetails { get; set; }
        public DbSet<ApproveReceipt> ApproveReceipts { get; set; }
        public DbSet<bankDeposit> bankDeposits { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<cashDeposit> cashDeposits { get; set; }
        public DbSet<ChequeInformation> ChequeInformations { get; set; }
        public DbSet<HandleBookReceipt> HandleBookReceipts { get; set; }
        public DbSet<BookDeliveryRequest> BookDeliveryRequests { get; set; }
        public DbSet<BookDeliveryRequestDetail> BookDeliveryRequestDetails { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<JobHistory> JobHistories { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Employee> Employees { get; set; }
    }
}
