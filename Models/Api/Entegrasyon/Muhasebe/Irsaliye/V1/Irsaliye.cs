//using Antibiotic.Database.Field;
//using Antibiotic.Database.Relation.Attributes;
//using Antibiotic.Extensions;
//using SasonBase.Sason.Models.ReportModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SasonWebs.Models.Api.Entegrasyon.Muhasebe.Irsaliye.V1
//{
//    public class Irsaliye : SasonBase.Sason.Tables.Table_FATURALAR.RawItem
//    {
//        public decimal ID { get; set; }

//        internal decimal SERVISID { get; set; }
//        internal decimal FATURATURID { get; set; }
//        public decimal FATURAVARLIKID { get; set; }

//        //[DbTargetField("ISEMIRNO")]
//        //public string IsEmirNo { get; set; }
//        [DbTargetField("FATURANO")]
//        public string FaturaNo { get; set; }
//        //[DbTargetField("ARACTIPI")]
//        //public string AracTipi { get; set; }
//        [DbTargetField("FATURAYIKESEN")]
//        public string IrsaliyeyiKesen { get; set; }
//        //[DbTargetField("TEVKIFATORAN")]
//        //public string TevkifatOran { get; set; }
//        //[DbTargetField("TEVKIFATTUTAR")]
//        //public decimal TevkifatTutar { get; set; }

//        [DbTargetField("CARIKOD")]
//        public string CariKod { get; set; }

//        public string IlgiliKisiAd { get; set; }
//        public string IlgiliKisiTelefon { get; set; }

//        [DbTargetField("IRSALIYENO")]
//        public string IrsaliyeNo { get; set; }


//        #region KdvDahilMi
//        public bool KdvDahilMi
//        {
//            get
//            {
//                return ToplamKdvTutari > 0;
//            }
//        }
//        #endregion


//        #region Iptal Durumu
//        decimal DURUMID { get; set; }
//        public bool IptalEdildi
//        {
//            get
//            {
//                return DURUMID == 0 ? true : false;
//            }
//        }
//        #endregion

//        //[DbTargetField("SONODEMETARIHI")]
//        //public DateTime SonOdemeTarihi { get; set; }

//        //[DbTargetField("ISLEMTARIHI")]
//        //public DateTime FaturaTarihSaati { get; set; }

//        [DbTargetField("IRSALIYETARIHI")]
//        public DateTime IrsaliyeTarihi { get; set; }

//        //[DbTargetField("INETTUTAR")]
//        //public decimal IscilikNetTutar { get; set; }
//        [DbTargetField("MNETTUTAR")]
//        public decimal MalzemeNetTutar { get; set; }
//        [DbTargetField("NETTUTAR")]
//        public decimal FaturaNetTutar { get; set; }
//        //[DbTargetField("DNETTUTAR")]
//        //public decimal DigerNetTutar { get; set; }

//        [DbTargetField("NETKDVTOPLAM")]
//        public decimal ToplamKdvTutari { get; set; }
//        [DbTargetField("TOPLAM")]
//        public decimal ToplamTutar { get; set; }

//        [ReadOnlyRelation]
//        [RelationCondition("ID", "FATURAID")]
//        public IEnumerable<IrsaliyeDetay> IrsaliyeDetaylar { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("FATURATURID", "ID")]
//        public ReportData_FATURATURLER FaturaTuru { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("app:ServiceId", "ServisId")]
//        [RelationCondition("app:Language", "DILKOD")]
//        public ReportData_VT_SERVISLER ServisInfo { get; set; }

//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("ISEMIRNO", "ISEMIRNO")]
//        //public ReportData_SERVISISEMIRLER IsEmirInfo { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("FATURAVARLIKID", "SERVISVARLIKID")]
//        [RelationCondition("app:ServiceId", "SERVISID")]
//        [RelationCondition("app:Language", "DILKOD")]
//        public ReportData_VT_SERVISVARLIKLAR CariInfo { get; set; }
//    }

//    public class IrsaliyeDetay : SasonBase.Sason.Tables.Table_FATURADETAYLAR.RawItem
//    {
//        Decimal ID { get; set; }
//        Decimal FATURAID { get; set; }

//        [DbTargetField("KOD")]
//        public string Kod { get; set; }
//        [DbTargetField("ACIKLAMA")]
//        public String Aciklama { get; set; }
//        [DbTargetField("BIRIMAD")]
//        public String BirimAdi { get; set; }
//        [DbTargetField("BIRIMFIYAT")]
//        public Decimal BirimFiyat { get; set; }
//        [DbTargetField("MIKTAR")]
//        public Decimal Miktar { get; set; }

//        //[DbTargetField("AYRISTIRMAORANI")]
//        //public Decimal AyristirmaOrani { get; set; }
//        [DbTargetField("TLFIYAT")]
//        public Decimal TLFiyati { get; set; }
//        [DbTargetField("INDIRIM")]
//        public Decimal IndirimOrani { get; set; }
//        [DbTargetField("BRUTTUTAR")]
//        public Decimal BrutTutar { get; set; }
//        [DbTargetField("TUTAR")]
//        public decimal Tutar { get; set; }
//        [DbTargetField("KDVORAN")]
//        public decimal KdvOrani { get; set; }

//        //[DbTargetField("MANKARTORAN")]
//        //public decimal KazanilanManKartOran { get; set; }
//        //[DbTargetField("MANKARTPUAN")]
//        //public decimal KazanilanManKartPuan { get; set; }
//        //[DbTargetField("MANKARTHARCANAN")]
//        //public decimal HarcananManKartPuan { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("KOD", "KOD")]
//        [RelationCondition("app:ServiceId", "SERVISID")]
//        [RelationCondition("app:Language", "DILKOD")]
//        ReportData_VT_SERVISSTOKLAR ServisStokInfo { get; set; }

//        //[ReadOnlyMappedRelation(CustomPkFieldName = "KOD")]
//        //[RelationCondition("KOD", "KOD")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //ReportData_VT_ISCILIKLER IscilikInfo { get; set; }

//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("KOD", "KOD")]
//        //[RelationCondition("app:ServiceId", "SERVISID")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //ReportData_VW_SERVISISCILIKLER ServisIscilikInfo { get; set; }

//        #region DepoAdi
//        public string DepoAdi
//        {
//            get
//            {
//                return ServisStokInfo?.DepoAdi;
//            }
//        }
//        #endregion

//        #region KalemAciklama (Parca, Iscilik, Diger)
//        public string KalemAciklama
//        {
//            get
//            {
//                string ret = "Diger";
//                if (ServisStokInfo.isNotNull())
//                    ret = "Parca";
//                //else if (IscilikInfo.isNotNull() || ServisIscilikInfo.isNotNull())
//                //    ret = "Iscilik";
//                else if (Kod.like("(*)"))
//                    ret = "Parca";
//                return ret;
//            }
//        }
//        #endregion
//    }
//}