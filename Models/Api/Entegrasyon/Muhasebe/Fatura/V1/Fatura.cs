//using Antibiotic.Database.Field;
//using Antibiotic.Database.Relation.Attributes;
//using Newtonsoft.Json;
//using SasonBase.Sason.Models.ReportModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Antibiotic.Extensions;

//namespace SasonWebs.Models.Api.Entegrasyon.Muhasebe.Fatura.V1
//{
//    public class FaturaInfo000 : SasonBase.Sason.Tables.Table_FATURALAR.RawItem
//    {
//        internal decimal ID { get; set; }
//        internal decimal SERVISID { get; set; }
//        [DbTargetField("ISLEMTARIHI")] public DateTime FaturaTarihSaati { get; set; }
//        [DbTargetField("FATURANO")] public string FaturaNo { get; set; }
//    }


//    public class Fatura : SasonBase.Sason.Tables.Table_FATURALAR.RawItem
//    {
//        public decimal ID { get; set; }

//        internal decimal SERVISID { get; set; }
//        internal decimal FATURATURID { get; set; }

//        public decimal FATURAVARLIKID { get; set; }

//        [DbTargetField("ISEMIRNO")]
//        public string IsEmirNo { get; set; }
//        [DbTargetField("FATURANO")]
//        public string FaturaNo { get; set; }
//        [DbTargetField("ARACTIPI")]
//        public string AracTipi { get; set; }
//        [DbTargetField("FATURAYIKESEN")]
//        public string FaturayiKesen { get; set; }
//        [DbTargetField("TEVKIFATORAN")]
//        public string TevkifatOran { get; set; }
//        [DbTargetField("TEVKIFATTUTAR")]
//        public decimal TevkifatTutar { get; set; }


//        [DbTargetField("FATURANOT")]
//        public String FaturaNot { get; set; }

//        [DbTargetField("TEVKIFATACIKLAMA")]
//        public string TevkifatAciklama { get; set; }


//        [DbTargetField("CARIKOD")]
//        public string CariKod { get; set; }

//        public string IlgiliKisiAd { get; set; }
//        public string IlgiliKisiTelefon { get; set; }

//        [DbTargetField("IRSALIYENO")]
//        public string IrsaliyeNo { get; set; }

//        [DbTargetField("IRSALIYETARIHI")]
//        public DateTime IrsaliyeTarihi { get; set; }

//        [DbTargetField("ICMALALTBILGI")]
//        public string AltBilgi { get; set; }

//        [DbTargetField("KUR")]
//        public Decimal Kur { get; set; }

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

//        [DbTargetField("SONODEMETARIHI")]
//        public DateTime SonOdemeTarihi { get; set; }
//        [DbTargetField("ISLEMTARIHI")]
//        public DateTime FaturaTarihSaati { get; set; }
//        [DbTargetField("INETTUTAR")]
//        public decimal IscilikNetTutar { get; set; }
//        [DbTargetField("MNETTUTAR")]
//        public decimal MalzemeNetTutar { get; set; }
//        [DbTargetField("DNETTUTAR")]
//        public decimal DigerNetTutar { get; set; }

//        [DbTargetField("NETTUTAR")]
//        public decimal FaturaNetTutar { get; set; }
//        [DbTargetField("NETKDVTOPLAM")]
//        public decimal ToplamKdvTutari { get; set; }
//        [DbTargetField("TOPLAM")]
//        public decimal ToplamTutar { get; set;}
        


//        [ReadOnlyRelation]
//        [RelationCondition("ID", "FATURAID")]
//        public IEnumerable<FaturaDetay> FaturaDetaylar { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("FATURATURID", "ID")]
//        public RptFaturaTuru FaturaTuru { get; set; }

//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("app:ServiceId", "ServisId")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //public ReportData_VT_SERVISLER ServisInfo_Old { get; set; }
//        //Yukarıdaki Burası İle Değiştirildi
//        [ReadOnlyMappedRelation]
//        [RelationCondition("app:ServiceId", "ServisId")]
//        public RptServislerInfo00 ServisInfo { get; set; }
        
//        [ReadOnlyMappedRelation]
//        [RelationCondition("ISEMIRNO", "ISEMIRNO")]
//        public RptServisIsEmirler IsEmirInfo { get; set; }

//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("FATURAVARLIKID", "SERVISVARLIKID")]
//        //[RelationCondition("app:ServiceId", "SERVISID")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //public ReportData_VT_SERVISVARLIKLAR CariInfo_Old { get; set; }
//        //Yukarıdaki Burası İle Değiştirildi
//        [ReadOnlyMappedRelation]
//        [RelationCondition("FATURAVARLIKID", "ID")]
//        //[RelationCondition("app:ServiceId", "SERVISID")]
//        public RptServisVarlik CariInfo { get; set; }

//        [ReadOnlyRelation]
//        [RelationCondition("ISEMIRNO", "ISEMIRNO")]
//        internal List<ReportData_VT_ISEMIRDETAYLARFATURA> IsEmirDetaylarFatura { get; set; }


//        [ReadOnlyRelation]
//        [RelationCondition("ID","FATURAID")]
//        public List<RptServisStokHareket01> Irsaliyeler { get; set; }


//        public override string ToString()
//        {
//            return $"{this.FaturaNo}, {Irsaliyeler.select(t=> t.IrsaliyeNo).joinNumeric(",")}";
//        }
//    }

//    public class FaturaDetay : SasonBase.Sason.Tables.Table_FATURADETAYLAR.RawItem
//    {
//        Decimal ID { get; set; }
//        Decimal FATURAID { get; set; }

//        [DbTargetField("KOD")]
//        public string Kod { get; set; }
//        //Original Malzeme Kodu İçin Kullanılıyor
//        public string Kod2 { get; set; }


//        [DbTargetField("ACIKLAMA")]
//        public String Aciklama { get; set; }
//        [DbTargetField("BIRIMAD")]
//        public String BirimAdi { get; set; }
//        [DbTargetField("BIRIMFIYAT")]
//        public Decimal BirimFiyat { get; set; }
//        [DbTargetField("MIKTAR")]
//        public Decimal Miktar { get; set; }


//        [DbTargetField("AYRISTIRMAORANI")]
//        public Decimal AyristirmaOrani { get; set; }
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


//        [DbTargetField("MANKARTORAN")]
//        public decimal KazanilanManKartOran { get; set; }
//        [DbTargetField("MANKARTPUAN")]
//        public decimal KazanilanManKartPuan { get; set; }
//        [DbTargetField("MANKARTHARCANAN")]
//        public decimal HarcananManKartPuan { get; set; }


//        [DbTargetField("ISCILIKTUTAREUR")]
//        public Decimal IscilikTutarEur { get; set; }
//        [DbTargetField("MALZEMETUTAREUR")]
//        public Decimal MalzemeTutarEur { get; set; }
//        [DbTargetField("PIYASAFATTUTAREUR")]
//        public Decimal PiyasaFaturaTutarEur { get; set; }
//        [DbTargetField("ISLETIMTUTAREUR")]
//        public Decimal IsletimTutarEur { get; set; }


//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("KOD", "KOD")]
//        //[RelationCondition("app:ServiceId", "SERVISID")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //ReportData_VT_SERVISSTOKLAR ServisStokInfo { get; set; }

//        //**********************************
//        //Yukarıdaki ServisStokInfo Daha Sonra Buraya Değiştirilecek
//        //**********************************
//        [ReadOnlyMappedRelation]
//        [RelationCondition("KOD", "KOD")]
//        [RelationCondition("app:ServiceId", "SERVISID")]
//        RptServisStokInfo03 ServisStokInfo { get; set; }

//        //Değiştirilemiyor ...
//        //[ReadOnlyMappedRelation(CustomPkFieldName = "KOD")]
//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("KOD", "KOD")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //ReportData_VT_ISCILIKLER IscilikInfo { get; set; }

//        [ReadOnlyMappedRelation]
//        [RelationCondition("KOD", "KOD")]
//        [RelationCondition("app:Language", "DILKOD")]
//        RptMtIscilik IscilikInfo { get; set; }

//        //[ReadOnlyMappedRelation]
//        //[RelationCondition("KOD", "KOD")]
//        //[RelationCondition("app:ServiceId", "SERVISID")]
//        //[RelationCondition("app:Language", "DILKOD")]
//        //ReportData_VW_SERVISISCILIKLER ServisIscilikInfo_Old { get; set; }
//        /// <summary>
//        /// Yukarıdakinin Yerine Burası Yazıldı
//        /// </summary>
//        [ReadOnlyMappedRelation]
//        [RelationCondition("KOD", "KOD")]
//        [RelationCondition("app:ServiceId", "SERVISID")]
//        RptServisIscilik ServisIscilikInfo { get; set; }

//        #region DepoAdi
//        public string DepoAdi
//        {
//            get
//            {
//                //return ServisStokInfo?.DepoAdi;
//                return ServisStokInfo?.ServisDepoAdi;  //Daha Sonra Buraya Değiştirilecek
//            }
//        }
//        #endregion

//        public bool OrijinalParca
//        {
//            get
//            {
//                bool ret = false;
//                if (ServisStokInfo.isNotNull())
//                    //ret = ServisStokInfo.StokTurId == 1;
//                    ret = ServisStokInfo.ServisStokTurId == 1;
//                return ret;
//            }
//        }

//        #region KalemAciklama (Parca, Iscilik, Diger)
//        public string KalemAciklama
//        {
//            get
//            {
//                string ret = "Diger";
//                if (ServisStokInfo.isNotNull())
//                    ret = "Parca";
//                else if (IscilikInfo.isNotNull() || ServisIscilikInfo.isNotNull())
//                    ret = "Iscilik";
//                else if (Kod.like("(*)"))
//                    ret = "Parca";
//                return ret;
//            }
//        }
//        #endregion
//    }
//}