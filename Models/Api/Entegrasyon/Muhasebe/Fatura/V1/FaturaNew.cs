using Antibiotic.Database.Field;
using Antibiotic.Database.Relation.Attributes;
using Newtonsoft.Json;
using SasonBase.Sason.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antibiotic.Extensions;
using Antibiotic.Database.Row;
using Antibiotic.Database.Attributes;
using Antibiotic.Database.Query.Statements;
using Antibiotic.Database.Query;
using System.Data;
using Antibiotic.Database.Query.Objects;
using SasonBase.Sason.Models.ViewModels;

namespace SasonWebs.Models.Api.Entegrasyon.Muhasebe.Fatura.V1
{
    public static class FaturaTools
    {
        private class XModel
        {
            public decimal FATURAID { get; set; }
            public string SERVISSTOKKOD { get; set; }
            public string ORJINALMALZEMEKOD { get; set; }
        }

        public static void RepairFaturalar(List<Fatura> faturalar, decimal servisId)
        {
            MethodReturn mr = new MethodReturn();
            int kontrol = 0;
            //http://localhost:59761/api/entegrasyon/muhasebe/fatura/debugkesilenfaturalistesi?serviceid=R546&servicepwd=mapbursa&startdate=20171006&finishdate=20171006
            //http://localhost:59761/api/sfat?faturaid=7968
            //http://localhost:59761/api/entegrasyon/muhasebe/fatura/debugkesilenfaturalistesi?serviceid=R540&servicepwd=esa4tr&startdate=20171001&finishdate=20171030

            #region İş Emirleri Orjinal Kod Bulma ve Yerleştirme İşlemi
            if (mr.ok())
            {
                List<Fatura> isEmirFaturalar = faturalar.where(t => t.FATURATURID == 1).toList();
                if (isEmirFaturalar.isNotEmpty())
                {
                    List<string> isEmirNolar = isEmirFaturalar.where(t => t.IsEmirNo.isNotNullOrWhiteSpace()).select(t => t.IsEmirNo).toList().Distinct().toList();

                    List<ReportData_VT_ISEMIRDETAYLARFATURA> isEmirDetaylarFatura = null;
                    if (isEmirNolar.isNotEmpty())
                        isEmirDetaylarFatura = R.Query<ReportData_VT_ISEMIRDETAYLARFATURA>(mr).Where(t => t.SERVISID == servisId && t.ISEMIRNO.In(isEmirNolar)).ToList();

                    if (mr.ok())
                    {
                        isEmirFaturalar.forEach(fatura =>
                        {
                            fatura.IsEmirDetaylarFatura = isEmirDetaylarFatura.where(t => t.ISEMIRNO == fatura.IsEmirNo).toList();
                        });

                        isEmirFaturalar.forEach(fatura =>
                        {
                            fatura.FaturaDetaylar.forEach(detay =>
                            {
                                ReportData_VT_ISEMIRDETAYLARFATURA foundKodDef = fatura.IsEmirDetaylarFatura.first(t => t.KOD == detay.Kod || t.ORJINALKOD == detay.Kod);
                                if (foundKodDef.isNotNull())
                                {
                                    if (detay.Kod == foundKodDef.KOD)
                                        detay.Kod2 = foundKodDef.ORJINALKOD;
                                    if (detay.Kod == foundKodDef.ORJINALKOD)
                                        detay.Kod2 = foundKodDef.KOD;

                                }

                                if (detay.Kod2.isNullOrWhiteSpace())
                                    detay.Kod2 = detay.Kod;
                                //CSEYREK: fatura icmal ise brüt tutar 0 geliyordu, bu nedenle icmallerde brüt tutarı tutara eşitledik.
                                
                                //List<Fatura> icmalFaturalari = faturalar.where(t => t.FATURATURID == 2).toList();
                                //List<FaturaDetay> icmalFaturalariDetaylar = new List<FaturaDetay>();
                                //icmalFaturalari.forEach(t => icmalFaturalariDetaylar.addRange(t.FaturaDetaylar));
                                //if (icmalFaturalariDetaylar.isNotEmpty())
                                //{
                                //    detay.BrutTutar = detay.Tutar;
                                //}
                                    
                                
                            });
                        });
                    }
                }
            }
            #endregion

            if (mr.ok())
            {
                List<Fatura> satisFaturalari = faturalar.where(t => t.FATURATURID == 3).toList();
                if (satisFaturalari.isNotEmpty())
                {
                    List<decimal> faturaids = satisFaturalari.select(t => t.ID).toList();

                    List<XModel> dsfaturalar = new List<XModel>();

                    faturaids.toPacks(995).forEach(pck =>
                    {
                        SasonWebAppPool.Get.EbaTestConnector.CreateQuery($@"
                            select
                                 fat.id FATURAID, SSTK.KOD SERVISSTOKKOD, omlz.kod ORJINALMALZEMEKOD
                            from (select * from faturalar) fat
                            left join servissiparismlzler ssm on ssm.faturaid = fat.id
                            left join servisstoklar sstk on sstk.id = ssm.servisstokid
                            left join malzemeler mlz on mlz.id = sstk.malzemeid
                            left join malzemeler omlz on omlz.id = mlz.orjinalmalzemeid
                            where fat.id in ({pck.Value.joinNumeric(",")})
                            ")
                            .GetDataTable(mr).ToModels<XModel>(mr).AppendToList(dsfaturalar);
                    });


                    satisFaturalari.forEach(fatura =>
                    {
                        fatura.FaturaDetaylar.forEach(faturaDetay =>
                        {
                            XModel found = dsfaturalar.first(t => t.ORJINALMALZEMEKOD == faturaDetay.UnformattedCode);
                            if (found.isNotNull())
                            {
                                if (faturaDetay.IsYansanayiKod)
                                    faturaDetay.Kod2 = found.SERVISSTOKKOD;
                            }
                        });
                    });


                    //List<string> kods = new List<string>();

                    //satisFaturalari.forEach(fatura =>
                    //{
                    //    fatura.FaturaDetaylar.forEach(faturaDetay =>
                    //    {
                    //        kods.set(faturaDetay.UnformattedCode);
                    //    });
                    //});

                    //List<ORJINALTOMALZEMEKOD> kodlar = ORJINALTOMALZEMEKOD.SelectOverloadFromKods(kods);

                    //satisFaturalari.forEach(fatura =>
                    //{
                    //    fatura.FaturaDetaylar.forEach(faturaDetay =>
                    //    {
                    //        ORJINALTOMALZEMEKOD found = null;// kodlar.first(t => fatura.SiparisMalzemeler.select(r => r.Kod).toList().contains(t.SEARCH_KOD)); // kodlar.first(t => t.SEARCH_KOD == faturaDetay.UnformattedCode);
                    //        if (found.isNull())
                    //        {
                    //            //if (faturaDetay.IsYansanayiKod)
                    //            //    faturaDetay.Kod2 = faturaDetay.Kod.Replace("(*)", "").Replace("Y", "");
                    //        }
                    //        else
                    //        {
                    //            if (faturaDetay.Kod.left(1) == "Y")
                    //            {

                    //            }
                    //            if (faturaDetay.IsYansanayiKod)
                    //                faturaDetay.Kod2 = found.FOUND_GKOD;
                    //        }
                    //    });
                    //});
                }
            }

            if (mr.ok())
            {
                //Dictionary<string, string> yildizToOrjinal = new Dictionary<string, string>();
                //faturalar.forEach(fatura =>
                //{
                //    fatura.FaturaDetaylar.forEach(faturaDetay =>
                //    {
                //        //Yıldızlı Parça Varsa Bunun Orjinal Kodu Bulunup Kod2 ye Atılacak
                //        if (faturaDetay.Kod.like("(*)"))
                //        {
                //            yildizToOrjinal.set(faturaDetay.Kod.KodToMalzemelerKod(), "");
                //        }
                //    });
                //});

                //if (yildizToOrjinal.isNotEmpty())
                //{
                //    DataTable dtb = SasonWebAppPool.Get.EbaTestConnector.CreateQuery(@"
                //        select
                //            yansan.kod YANSANKOD, orjinal.kod ORJINALKOD
                //        from (select id, kod from malzemeler where kod in (" + yildizToOrjinal.keys().join(",") + @")) yansan
                //        left join malzemeler orjinal on ORJINAL.ORJINALMALZEMEID = yansan.id
                //    ").GetDataTable();
                //    foreach (DataRow dtr in dtb.Rows)
                //    {
                //        yildizToOrjinal.set(dtr["YANSANKOD"].toString(), dtr["ORJINALKOD"].toString());
                //    }

                //    faturalar.forEach(fatura =>
                //    {
                //        fatura.FaturaDetaylar.forEach(faturaDetay =>
                //        {
                //            //Yıldızlı Parça Varsa Bunun Orjinal Kodu Bulunup Kod2 ye Atılacak
                //            if (faturaDetay.Kod.like("(*)"))
                //            {
                //                string foundOrjinalKod = yildizToOrjinal.find(faturaDetay.Kod.KodToMalzemelerKod());
                //                if (foundOrjinalKod.isNotNullOrWhiteSpace() && foundOrjinalKod != faturaDetay.Kod.KodToMalzemelerKod())
                //                    faturaDetay.Kod2 = foundOrjinalKod;
                //            }

                //            if (faturaDetay.Kod2.isNullOrWhiteSpace())
                //                faturaDetay.Kod2 = faturaDetay.Kod;
                //        });
                //    });



                    //dbSelect select = new dbSelect("MALZEMELER").select("kod")
                    //    .where(
                    //            qt.field("ORJINALMALZEMEID").inn(
                    //                new dbSelect("MALZEMELER").select("id").where(qt.field("KOD").inn(yildizToOrjinal.keys()))
                    //            )
                    //        );
                    //DataTable dtb = SasonWebAppPool.Get.EbaTestConnector.GetDataTable(select).Result;



                    //dbSelect yansan = new dbSelect("malzemeler").select("id", "kod").where(qt.field("kod").inn(yildizToOrjinal.keys())).alias("yansan")
                    //    .leftJoin(new dbTable("malzemeler"), qt ;

                    //dbSelect select = new dbSelect().select(qt.field(yansan, "kod"), qt.field(  );

                //}
            }

            #region Dis Hizmet Faturalarinda Cari Bilgileri Gelmeyenler İçin Hazırlandı
            {
                if (mr.ok())
                {
                    // Dış Hizmet Alım Faturalarında Cari Bilgileri Gelmiyormuş. Veritabanında ve Projelerde Değişiklik Yapmamak İçin Burası Eklendi.
                    List<string> dishizmetvnolist = faturalar.where(t => t.FATURATURID == 5 && t.CariInfo.isNull()).select(t => t.VergiNo).ToList();
                    if (dishizmetvnolist.isNotEmpty())
                    {
                        List<RptServisVarlik> dishizmetservisvarliklar = R.Query<RptServisVarlik>(mr).Where(t => t.VergiNo.In(dishizmetvnolist) && t.SERVISID == servisId).ToList();
                        if (mr.ok())
                        {
                            faturalar.forEach(fatura =>
                            {
                                if (fatura.CariInfo.isNull())
                                    fatura.CariInfo = dishizmetservisvarliklar.first(t => t.VergiNo == fatura.VergiNo);
                            });
                        }
                    }
                }
            }
            #endregion

            #region İcmal Faturalarının Detaylarını Verebilmek İçin Hazırlandı
            {
                if (mr.ok())
                {
                    List<Fatura> icmalFaturalari = faturalar.where(t => t.FATURATURID == 2).toList();
                    List<FaturaDetay> icmalFaturalariDetaylar = new List<FaturaDetay>();
                    icmalFaturalari.forEach(t => icmalFaturalariDetaylar.addRange(t.FaturaDetaylar));
                    if (icmalFaturalariDetaylar.isNotEmpty())
                    {
                        List<string> garantiNumaralari = icmalFaturalariDetaylar.select(t => t.Kod).toList();
                        if (garantiNumaralari.isNotEmpty())
                        { 
                            List<RptAyristirmalar> ayristirmalar = R.Query<RptAyristirmalar>(mr).Where(t => t.CLAIMNO.In(garantiNumaralari)).ToList();
                            faturalar.forEach(fat =>                            {

                                fat.FaturaDetaylar.forEach(det =>
                                {
                                    det.IcmalDetaylari = ayristirmalar.first(t => t.CLAIMNO == det.Kod)?.Detaylar;//.where(t=> t.Kod.isNotNullOrWhiteSpace());                                    
                                    //CSEYREK: fatura icmal ise brüt tutar 0 geliyordu, bu nedenle icmallerde brüt tutarı tutara eşitledik.
                                      //det.BrutTutar =det.Tutar;
                                    det.IcmalBrutTutar = det.Tutar;
                                });
                            });
                        }
                    }
                }
            }
            #endregion

            faturalar.forEach(fatura =>
            {
                fatura.FaturaDetaylar.forEach(detay =>
                {
                    if (detay.Kod2.isNullOrWhiteSpace())
                        detay.Kod2 = detay.Kod;
                });
            });
        }
    }




    public class FaturaInfo000 : SasonBase.Sason.Tables.Table_FATURALAR.RawItem
    {
        internal decimal ID { get; set; }
        internal decimal SERVISID { get; set; }
        [DbTargetField("ISLEMTARIHI")] public DateTime FaturaTarihSaati { get; set; }
        [DbTargetField("FATURANO")] public string FaturaNo { get; set; }
    }

    //public class NewFatuar : Fatura
    //{

    //}
    
    public class Fatura : SasonBase.Sason.Tables.Table_FATURALAR.RawItem
    {
        public decimal ID { get; set; }

        internal decimal SERVISID { get; set; }
        internal decimal FATURATURID { get; set; }
        [DbTargetField("VNO")] internal String VergiNo { get; set; }

        public decimal FATURAVARLIKID { get; set; }

        [DbTargetField("ISEMIRNO")]
        public string IsEmirNo { get; set; }
        [DbTargetField("FATURANO")]
        public string FaturaNo { get; set; }
        [DbTargetField("ARACTIPI")]
        public string AracTipi { get; set; }
        [DbTargetField("FATURAYIKESEN")]
        public string FaturayiKesen { get; set; }
        [DbTargetField("TEVKIFATORAN")]
        public string TevkifatOran { get; set; }
        [DbTargetField("TEVKIFATTUTAR")]
        public decimal TevkifatTutar { get; set; }


        [DbTargetField("FATURANOT")]
        public String FaturaNot { get; set; }

        [DbTargetField("TEVKIFATACIKLAMA")]
        public string TevkifatAciklama { get; set; }

        [DbTargetField("CARIKOD")]
        public string CariKod { get; set; }

        public string IlgiliKisiAd { get; set; }
        public string IlgiliKisiTelefon { get; set; }

        [DbTargetField("IRSALIYENO")]
        public string IrsaliyeNo { get; set; }

        [DbTargetField("IRSALIYETARIHI")]
        public DateTime IrsaliyeTarihi { get; set; }

        [DbTargetField("ICMALALTBILGI")]
        public string AltBilgi { get; set; }

        [DbTargetField("KUR")]
        public Decimal Kur { get; set; }

        #region KdvDahilMi
        public bool KdvDahilMi
        {
            get
            {
                return ToplamKdvTutari > 0;
            }
        }
        #endregion

        #region Iptal Durumu
        decimal DURUMID { get; set; }
        public bool IptalEdildi
        {
            get
            {
                return DURUMID == 0 ? true : false;
            }
        }
        #endregion

        [DbTargetField("SONODEMETARIHI")]
        public DateTime SonOdemeTarihi { get; set; }
        [DbTargetField("ISLEMTARIHI")]
        public DateTime FaturaTarihSaati { get; set; }
        [DbTargetField("INETTUTAR")]
        public decimal IscilikNetTutar { get; set; }
        [DbTargetField("MNETTUTAR")]
        public decimal MalzemeNetTutar { get; set; }
        [DbTargetField("DNETTUTAR")]
        public decimal DigerNetTutar { get; set; }

        [DbTargetField("BRUTTOPLAM")]
        public decimal BrutToplam { get; set; }

        [DbTargetField("NETTUTAR")]
        public decimal FaturaNetTutar { get; set; }
        [DbTargetField("NETKDVTOPLAM")]
        public decimal ToplamKdvTutari { get; set; }
        [DbTargetField("TOPLAM")]
        public decimal ToplamTutar { get; set; }

        protected override void Initialized(ItemRawModel ownerItem)
        {
            if (FaturaTuru != null)
            {
                switch (FaturaTuru.ID)
                {
                    case 4: //Alış Faturası
                        {
                            FaturaNetTutar = FaturaNetTutar - ToplamKdvTutari;
                            ToplamTutar = FaturaNetTutar + ToplamKdvTutari;
                        }
                        break;
                    case 5: //Dış Hizmet Faturası
                        {
                            ToplamTutar = FaturaNetTutar + ToplamKdvTutari;
                        }
                        break;
                }
            }
        }


        [ReadOnlyRelation]
        [RelationCondition("ID", "FATURAID")]
        public IEnumerable<FaturaDetay> FaturaDetaylar { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("FATURATURID", "ID")]
        public RptFaturaTuru FaturaTuru { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("app:ServiceId", "ServisId")]
        public RptServislerInfo00 ServisInfo { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("ISEMIRNO", "ISEMIRNO")]
        [RelationCondition("app:ServiceId", "SERVISID")]
        public RptServisIsEmirler IsEmirInfo { get; set; }

        //[ReadOnlyMappedRelation]
        //[RelationCondition("FATURAVARLIKID", "ID")]
        //public RptServisVarlik CariInfo { get; set; }
        [ReadOnlyMappedRelation]
        [RelationCondition("VNO", "VERGINO")]
        [RelationCondition("app:ServiceId", "SERVISID")]
        public RptServisVarlik CariInfo { get; set; }


        //[ReadOnlyRelation]
        //[RelationCondition("ISEMIRNO", "ISEMIRNO")]
        internal List<ReportData_VT_ISEMIRDETAYLARFATURA> IsEmirDetaylarFatura { get; set; }

        [ReadOnlyRelation]
        [RelationCondition("ID", "FATURAID")]
        public List<RptServisStokHareket01> Irsaliyeler { get; set; }

        public override string ToString()
        {
            return $"{this.FaturaNo}, {Irsaliyeler.select(t => t.IrsaliyeNo).joinNumeric(",")}";
        }
    }

    public class FaturaDetay : SasonBase.Sason.Tables.Table_FATURADETAYLAR.RawItem
    {
        internal Decimal ID { get; set; }
        internal Decimal FATURAID { get; set; }

        //internal String STOKKOD { get; set; }

        [DbTargetField("KOD")]
        public string Kod { get; set; }
        //Original Malzeme Kodu İçin Kullanılıyor
        [DbTargetField("STOKKOD")]
        public string Kod2 { get; set; }

        internal bool IsYansanayiKod { get => Kod.StartsWith("(*)"); }

        internal string UnformattedCode
        {
            get
            {
                string retKod = Kod;
                int len = retKod.Length;

                bool nokta = retKod.IndexOf(".") > 0;
                bool tire  = retKod.IndexOf("-") > 0;
                bool yansan = IsYansanayiKod;

                if (nokta && tire)
                    retKod = retKod.Replace(".", "").Replace("-", "");
                
                if (yansan)
                    retKod = retKod.Replace("(*)", "").Replace("Y","");

                //if (YansanayiKodu)
                //    return Kod.Replace("(*)", "").Replace("Y", "").Replace(".", "").Replace("-", "");
                return retKod;
            }
        }


        [DbTargetField("ACIKLAMA")]
        public String Aciklama { get; set; }
        [DbTargetField("BIRIMAD")]
        public String BirimAdi { get; set; }
        [DbTargetField("BIRIMFIYAT")]
        public Decimal BirimFiyat { get; set; }
        [DbTargetField("MIKTAR")]
        public Decimal Miktar { get; set; }


        [DbTargetField("AYRISTIRMAORANI")]
        public Decimal AyristirmaOrani { get; set; }
        [DbTargetField("TLFIYAT")]
        public Decimal TLFiyati { get; set; }
        [DbTargetField("INDIRIM")]
        public Decimal IndirimOrani { get; set; }
        [DbTargetField("BRUTTUTAR")]
        public Decimal BrutTutar { get; set; }
        //Icmal faturaları için ekledi.
        public Decimal IcmalBrutTutar { get; set; }
        [DbTargetField("TUTAR")]
        public decimal Tutar { get; set; }
        [DbTargetField("KDVORAN")]
        public decimal KdvOrani { get; set; }


        [DbTargetField("MANKARTORAN")]
        public decimal KazanilanManKartOran { get; set; }
        [DbTargetField("MANKARTPUAN")]
        public decimal KazanilanManKartPuan { get; set; }
        [DbTargetField("MANKARTHARCANAN")]
        public decimal HarcananManKartPuan { get; set; }


        [DbTargetField("ISCILIKTUTAREUR")]
        public Decimal IscilikTutarEur { get; set; }
        [DbTargetField("MALZEMETUTAREUR")]
        public Decimal MalzemeTutarEur { get; set; }
        [DbTargetField("PIYASAFATTUTAREUR")]
        public Decimal PiyasaFaturaTutarEur { get; set; }
        [DbTargetField("ISLETIMTUTAREUR")]
        public Decimal IsletimTutarEur { get; set; }

        decimal TURID { get; set; }

        [ReadOnlyMappedRelation]
        [RelationCondition("KOD", "KOD")]
        [RelationCondition("app:ServiceId", "SERVISID")]
        RptServisStokInfo03 ServisStokInfo { get; set; }


        public IEnumerable<RptAyristirmalarDetay> IcmalDetaylari { get; set; }

        #region DepoAdi
        public string DepoAdi
        {
            get
            {
                return ServisStokInfo?.ServisDepoAdi;
            }
        }
        #endregion

        public bool OrijinalParca
        {
            get
            {
                bool ret = false;
                if (ServisStokInfo.isNotNull())
                    ret = ServisStokInfo.ServisStokTurId == 1;
                return ret;
            }
        }

        #region KalemAciklama (Parca, Iscilik, Diger)
        public string KalemAciklama
        {
            get
            {
                string ret = "Diger";
                if (ServisStokInfo.isNotNull())
                    ret = "Parca";
                else if (TURID == 2)
                    ret = "Iscilik";
                else if (Kod.like("(*)"))
                    ret = "Parca";
                return ret;
            }
        }
        #endregion
    }
}