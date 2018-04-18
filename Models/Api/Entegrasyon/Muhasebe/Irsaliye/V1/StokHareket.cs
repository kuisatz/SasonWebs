using Antibiotic.Database.Field;
using Antibiotic.Database.Relation.Attributes;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.ReportModels.StokHareket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SasonWebs.Models.Api.Entegrasyon.Muhasebe.Irsaliye.V1
{
    public class IrsaliyeInfo000 : SasonBase.Sason.Tables.Table_SERVISSTOKHAREKETLER.RawItem
    {
        internal Decimal ID { get; set; }
        internal Decimal SERVISID { get; set; }
        internal Decimal SERVISSTOKISLEMID { get; set; }
        internal Decimal FATURAID { get; set; }
        [DbTargetField("TARIH")] public DateTime Tarih { get; set; }
        [DbTargetField("IRSALIYENO")] public String IrsaliyeNo { get; set; }
    }

    /// <summary>
    /// Faturalandırılmamış Alış İrsaliyeleri Web Servisi
    /// </summary>
    public class Irsaliye : SasonBase.Sason.Tables.Table_SERVISSTOKHAREKETLER.RawItem
    {
        public Decimal ID { get; set; }
        public Decimal SERVISSIPARISID { get; set; }

        internal Decimal SERVISSTOKISLEMID { get; set; }
        [ReadOnlyMappedRelation]
        [RelationCondition("SERVISSTOKISLEMID","ID")]
        RptServisStokIslemInfo02 ServisStokIslemInfo { get; set; }
        public string IslemKodu { get { return ServisStokIslemInfo?.Kod; } }
        public string IslemAdi { get { return ServisStokIslemInfo?.Adi; } }

        [DbTargetField("TARIH")] public DateTime Tarih { get; set; }
        [DbTargetField("BLGTARIH")] public DateTime BelgeTarihi { get; set; }
        [DbTargetField("BLGNO")] public String BelgeNo { get; set; }

        protected virtual Decimal PARABIRIMID { get; set; }
        [ReadOnlyMappedRelation]
        [RelationCondition("PARABIRIMID", "ID")]
        RptParaBirimiInfo ParaBirimiInfo { get; set; }
        public string ParaBirimi { get { return ParaBirimiInfo?.Kod; } }

        //public String ACIKLAMA { get; set; }
        protected virtual Decimal SERVISVARLIKID { get; set; }
        //protected virtual Decimal SERVISDEPOID { get; set; }
        //protected virtual Decimal SERVISDEPORAFID { get; set; }
        //protected virtual Decimal DURUMID { get; set; }
        internal Decimal SERVISID { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("app:ServiceId", "ServisId")]
        public RptServislerInfo00 ServisInfo { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("BLGTARIH", "TARIH")]
        [RelationCondition("USD", "CURRENCYCODE")]
        //public RptVXKurlar KurDolarInfo { get; set; }
        public RptVTKurlar KurDolarInfo { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("BLGTARIH", "TARIH")]
        [RelationCondition("EUR", "CURRENCYCODE")]
        //public RptVXKurlar KurEuroInfo { get; set; }
        public RptVTKurlar KurEuroInfo { get; set; }

        [DbTargetField("IRSALIYENO")] public String IrsaliyeNo { get; set; }
        //protected virtual String SERVISGARANTINO { get; set; }
        //protected virtual Decimal IRSALIYEONAY { get; set; }

        internal Decimal FATURAID { get; set; }
        //[ReadOnlyMappedRelation]
        //[RelationCondition("FATURAID", "ID")]
        //RptFatura FaturaInfo { get; set; }
        //public string FaturaNo { get { return FaturaInfo?.FaturaNo; } }


        //[ReadOnlyMappedRelation]
        //[RelationCondition("SERVISVARLIKID", "SERVISVARLIKID")]
        //[RelationCondition("app:ServiceId", "SERVISID")]
        //[RelationCondition("app:Language", "DILKOD")]
        //public ReportData_VT_SERVISVARLIKLAR CariInfo { get; set; }
        /// <summary>
        /// Yukarıdaki İle Değiştirildi
        /// </summary>
        [ReadOnlyMappedRelation]
        [RelationCondition("SERVISVARLIKID", "ID")]
        //[RelationCondition("app:ServiceId", "SERVISID")]
        //[RelationCondition("app:Language", "DILKOD")]
        public RptServisVarlik CariInfo { get; set; }

        [ReadOnlyRelation]
        [RelationCondition("ID", "SERVISSTOKHAREKETID")]
        public List<RptServisStokHareketDetay> HareketDetaylar { get; set; }
    }
}