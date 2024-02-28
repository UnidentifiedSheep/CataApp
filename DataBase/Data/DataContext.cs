﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataBase.Data;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agent> Agents { get; set; }

    public virtual DbSet<AgentTransaction> AgentTransactions { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<MainCat> MainCats { get; set; }

    public virtual DbSet<MainCatPrice> MainCatPrices { get; set; }

    public virtual DbSet<MainName> MainNames { get; set; }

    public virtual DbSet<ProdMainGroup> ProdMainGroups { get; set; }

    public virtual DbSet<Prodaja> Prodajas { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<ZakMainGroup> ZakMainGroups { get; set; }

    public virtual DbSet<Zakupka> Zakupkas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("DataSource=C:\\Users\\Shep\\Desktop\\data.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.ToTable("agents");

            entity.HasIndex(e => e.Id, "IX_agents_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsZak).HasColumnName("is_zak");
            entity.Property(e => e.Name)
                .HasDefaultValue(" ")
                .HasColumnName("name");
        });

        modelBuilder.Entity<AgentTransaction>(entity =>
        {
            entity.ToTable("agent_transactions");

            entity.HasIndex(e => e.Id, "IX_agent_transactions_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.TransactionDatatime).HasColumnName("transaction_datatime");
            entity.Property(e => e.TransactionStatus).HasColumnName("transaction_status");
            entity.Property(e => e.TransactionSum).HasColumnName("transaction_sum");

            entity.HasOne(d => d.Agent).WithMany(p => p.AgentTransactions).HasForeignKey(d => d.AgentId);

            entity.HasOne(d => d.CurrencyNavigation).WithMany(p => p.AgentTransactions).HasForeignKey(d => d.Currency);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");

            entity.HasIndex(e => e.Id, "IX_currencies_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CurrencyName)
                .HasDefaultValue(" ")
                .HasColumnName("currency_name");
            entity.Property(e => e.ToUsd).HasColumnName("to_usd");
        });

        modelBuilder.Entity<MainCat>(entity =>
        {
            entity.ToTable("main_cat");

            entity.HasIndex(e => e.Id, "IX_main_cat_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Name)
                .HasDefaultValue("  ")
                .HasColumnName("name");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.UniId).HasColumnName("uni_id");
            entity.Property(e => e.UniValue).HasColumnName("uni_value");

            entity.HasOne(d => d.Producer).WithMany(p => p.MainCats)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Uni).WithMany(p => p.MainCats).HasForeignKey(d => d.UniId);
        });

        modelBuilder.Entity<MainCatPrice>(entity =>
        {
            entity.ToTable("main_cat_prices");

            entity.HasIndex(e => e.Id, "IX_main_cat_prices_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.MainCatId).HasColumnName("main_cat_id");
            entity.Property(e => e.Price).HasColumnName("price");

            entity.HasOne(d => d.Currency).WithMany(p => p.MainCatPrices).HasForeignKey(d => d.CurrencyId);

            entity.HasOne(d => d.MainCat).WithMany(p => p.MainCatPrices).HasForeignKey(d => d.MainCatId);
        });

        modelBuilder.Entity<MainName>(entity =>
        {
            entity.HasKey(e => e.UniId);

            entity.ToTable("main_name");

            entity.HasIndex(e => e.UniId, "IX_main_name_uni_id").IsUnique();

            entity.Property(e => e.UniId).HasColumnName("uni_id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<ProdMainGroup>(entity =>
        {
            entity.ToTable("prod_main_group");

            entity.HasIndex(e => e.Id, "IX_prod_main_group_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Datetime).HasColumnName("datetime");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Agent).WithMany(p => p.ProdMainGroups)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Currency).WithMany(p => p.ProdMainGroups)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Prodaja>(entity =>
        {
            entity.ToTable("prodaja");

            entity.HasIndex(e => e.Id, "IX_prodaja_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.MainCatId).HasColumnName("main_cat_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ProdajaId).HasColumnName("prodaja_id");

            entity.HasOne(d => d.MainCat).WithMany(p => p.Prodajas)
                .HasForeignKey(d => d.MainCatId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ProdajaNavigation).WithMany(p => p.Prodajas).HasForeignKey(d => d.ProdajaId);
        });

        modelBuilder.Entity<Producer>(entity =>
        {
            entity.ToTable("producer");

            entity.HasIndex(e => e.Id, "IX_producer_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProducerName).HasColumnName("producer_name");
        });

        modelBuilder.Entity<ZakMainGroup>(entity =>
        {
            entity.ToTable("zak_main_group");

            entity.HasIndex(e => e.Id, "IX_zak_main_group_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgentId).HasColumnName("agent_id");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Datetime).HasColumnName("datetime");
            entity.Property(e => e.TotalSum).HasColumnName("total_sum");

            entity.HasOne(d => d.Agent).WithMany(p => p.ZakMainGroups)
                .HasForeignKey(d => d.AgentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Currency).WithMany(p => p.ZakMainGroups)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Zakupka>(entity =>
        {
            entity.ToTable("zakupka");

            entity.HasIndex(e => e.Id, "IX_zakupka_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.MainCatId).HasColumnName("main_cat_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.ZakId).HasColumnName("zak_id");

            entity.HasOne(d => d.MainCat).WithMany(p => p.Zakupkas)
                .HasForeignKey(d => d.MainCatId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Zak).WithMany(p => p.Zakupkas).HasForeignKey(d => d.ZakId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
