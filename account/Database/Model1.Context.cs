//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace account.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class accountEntities : DbContext
    {
        public accountEntities()
            : base("name=accountEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<user_login> user_login { get; set; }
        public virtual DbSet<bill_transaction> bill_transaction { get; set; }
        public virtual DbSet<bill_transaction_detail> bill_transaction_detail { get; set; }
        public virtual DbSet<bill_confirm_slip> bill_confirm_slip { get; set; }
    }
}
