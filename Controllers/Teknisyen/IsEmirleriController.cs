using Antibiotic.Database.Field;
using Antibiotic.Database.Relation.Attributes;
using Antibiotic.Extensions;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SasonBase.Sason.Models.PdksModels;
using SasonBase.Sason.Models.ReportModels;
using SasonBase.Sason.Models.TableModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Antibiotic.Database.Row;
using SasonBase.Sason.Models.TeknisyenModels;

namespace SasonWebs.Controllers
{
    public class IsEmirleriController : _Base._SasonController
    {
        [HttpGet]
        public object Get(DataSourceLoadOptions loadOptions)
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            List<Teknisyen_ViewServisIsEmirler> servisIsEmirleri = null;
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                servisIsEmirleri = Teknisyen_ViewServisIsEmirler.Select_Servis_IsEmirler_View(session.ServisId, session.ServisTeknisyenId);
            return DataSourceLoader.Load(servisIsEmirleri, loadOptions);
        }

        [HttpGet]
        public object IsEmirIslemler(decimal id, DataSourceLoadOptions loadOptions)
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            List<Teknisyen_ViewServisIsEmirIslemler> servisIsEmirIslemleri = null;
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                servisIsEmirIslemleri = Teknisyen_ViewServisIsEmirIslemler.Select_IsEmir_Islemler_View(id, session.ServisTeknisyenId);
            return DataSourceLoader.Load(servisIsEmirIslemleri, loadOptions);
        }

        [HttpGet]
        public object IsEmirIslemIscilikler(decimal id, DataSourceLoadOptions loadOptions)
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            List<Teknisyen_ViewServisIsEmirIslemIscilikler> iscilikler = null;
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
                iscilikler = Teknisyen_ViewServisIsEmirIslemIscilikler.Select_IsEmirIslem_Iscilikleri_View(id, session.ServisTeknisyenId);
            return DataSourceLoader.Load(iscilikler, loadOptions);
        }

        //[HttpPost]
        //public IActionResult GetTeknisyenIsEmirIslemIscilikData(decimal isEmirIslemIscilikId)
        //{
        //    SasonWebSession session = new SasonWebSession(HttpContext.Session);
        //    List<Teknisyen_ViewServisIsEmirIslemIscilikler> iscilikler = null;
        //    using (SasonWebAppPool appPool = SasonWebAppPool.Create)
        //        iscilikler = TeknisyenHareket.Select_IsEmirIslemIscilik_TeknisyenHareketleri.Select_IsEmirIslem_Iscilikleri_View(id, session.ServisTeknisyenId);
        //    return DataSourceLoader.Load(iscilikler, loadOptions);
        //}



        //private class DATA
        //{
        //    public decimal TargetHareketId { get; set; }
        //}

        /// <summary>
        /// Seçilmiş Olan İş Emir İşçiliğe Onay Veren Yer
        /// </summary>
        /// <param name="ustBilgiInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult IsEmirIslemIscilikSorgula(UstBilgiInfo ustBilgiInfo)
        {
            MethodReturn mr = new MethodReturn();

            string exceptionString = "";
            object data = null;
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                List<TeknisyenHareket> foundHareketler = TeknisyenHareket.GetTeknisyenGirisVeyaDevamBekleyenHareketler(session.ServisTeknisyenId);
                if (foundHareketler.isNotEmpty())
                {
                    HareketUstBilgi ustBilgi = HareketUstBilgi.Select_IsEmirIslemIscilik_UstHareketBilgisi(ustBilgiInfo.IsEmirIslemIscilikId);
                    if (ustBilgi.isNotNull())
                    {
                        TeknisyenHareket foundGirisHareketi = foundHareketler.first(t => t.DURUM == TeknisyenHareketDurum.Giris);
                        if (foundGirisHareketi.isNotNull())
                        {
                            if (foundGirisHareketi.THRKUSTBILGIID != ustBilgi.ID)
                                exceptionString = "Başka Bir İş Üzerinde Çalışırken Buraya Giremezsiniz";
                            else
                                data = new { TargetHareketId = foundGirisHareketi.ID };
                        }
                    }
                }
            }

            if (exceptionString.isNotNullOrWhiteSpace())
                return WmrException(exceptionString);
            else
                return WmrResult(data, mr);
        }

        /// <summary>
        /// Teknisyenin Aktif Durumuna Göre Neden Durumlarını Verir
        /// </summary>
        /// <param name="servisTeknisyenId"></param>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet]
        public object GetTeknisyenNedenler(decimal isEmirIscilikId, decimal targetTeknisyenHareketId, DataSourceLoadOptions loadOptions)
        {
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            decimal servisTeknisyenId = session.ServisTeknisyenId;
            List<HareketNeden> nedenler = new List<HareketNeden>();
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                TeknisyenHareket foundHareket = TeknisyenHareket.GetAktifHareket(isEmirIscilikId, servisTeknisyenId);
                if (foundHareket.isNotNull())
                {
                    if (foundHareket.DURUM == TeknisyenHareketDurum.Giris)
                    {
                        nedenler = R.Query<HareketNeden>().Where(t => t.HAREKETTIPI == HareketTipi.Cikis).ToList();
                        nedenler.forEach(neden => neden.NedenResim = "../images/waiting.png");
                        nedenler.forEach(neden =>
                        {
                            if (neden.FORMATI == NedenFormati.Cikis_IsBitisi)
                                neden.NedenResim = "../images/completed.png";
                        });
                    }
                    else if (foundHareket.DURUM == TeknisyenHareketDurum.DevamBekliyor)
                    {
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsDevami)).ToList();
                        nedenler.forEach(neden => neden.NedenResim = "../images/nedengiris.png");
                    }
                }
                else
                {
                    List<TeknisyenHareket> hareketler = TeknisyenHareket.Select_IsEmirIslemIscilik_TeknisyenHareketleri(isEmirIscilikId);
                    TeknisyenHareket girisHareketi = hareketler.first(t => t.DURUM == TeknisyenHareketDurum.Giris);
                    if (hareketler.isEmpty() || girisHareketi.isNull())
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsBaslangici)).ToList();
                    else
                        nedenler = R.Query<HareketNeden>().Where(t => t.FORMATI.In(NedenFormati.Giris_IsYardimi)).ToList();

                    nedenler.forEach(neden => neden.NedenResim = "../images/nedengiris.png");
                }
            }
            object result = DataSourceLoader.Load(nedenler, loadOptions);
            return result;
        }



        [HttpPost]
        public IActionResult UpdateTeknisyenHareket(UstBilgiInfo ustBilgiInfo)// TeknisyenHareket hareket)// decimal ServisId, decimal ServisTeknisyenId)
        {
            IActionResult result = null;
            MethodReturn mr = new MethodReturn();
            SasonWebSession session = new SasonWebSession(HttpContext.Session);
            using (SasonWebAppPool appPool = SasonWebAppPool.Create)
            {
                HareketUstBilgi ustBilgi = R.Query<HareketUstBilgi>(mr).First(t => t.ISEMIRISLEMISCILIKID == ustBilgiInfo.IsEmirIslemIscilikId);
                if (ustBilgi.isNotNull())
                {
                    TeknisyenHareket teknisyenHareket = null;
                    if (ustBilgiInfo.HareketTipi == HareketTipi.Cikis)
                    {
                        teknisyenHareket = TeknisyenHareket.GetTeknisyenCikisiYapilmamisGirisHareketi(ustBilgi, session.ServisTeknisyenId);//  .GetTeknisyenGirisVeyaDevamBekleyenHareket(session.ServisTeknisyenId);
                    }

                    HareketNeden neden = R.Query<HareketNeden>().First(t => t.ID == ustBilgiInfo.NedenId);

                    switch (ustBilgiInfo.HareketTipi)
                    {
                        case HareketTipi.Giris:
                            teknisyenHareket = new TeknisyenHareket();

                            teknisyenHareket.THRKUSTBILGIID = ustBilgi.ID;
                            teknisyenHareket.SERVISTEKNISYENID = session.ServisTeknisyenId;

                            teknisyenHareket.GIRISNEDENID = (int)ustBilgiInfo.NedenId;
                            teknisyenHareket.GIRISTARIHI = DateTime.Now;
                            teknisyenHareket.DURUM = TeknisyenHareketDurum.Giris;

                            if (neden.FORMATI == NedenFormati.Giris_IsDevami)
                            {
                                TeknisyenHareket parentHareket = TeknisyenHareket.GetAktifHareket(ustBilgiInfo.IsEmirIslemIscilikId, session.ServisTeknisyenId);
                                parentHareket.DURUM = TeknisyenHareketDurum.Cikis;
                                parentHareket.Update();
                            }
                            break;
                        case HareketTipi.Cikis:
                            teknisyenHareket.CIKISNEDENID = (int)ustBilgiInfo.NedenId;
                            teknisyenHareket.CIKISTARIHI = DateTime.Now;
                            teknisyenHareket.DURUM = TeknisyenHareketDurum.Cikis;
                            if (neden.FORMATI != NedenFormati.Cikis_IsBitisi)
                                teknisyenHareket.DURUM = TeknisyenHareketDurum.DevamBekliyor;
                            //ustBilgi.CALISILANSURE+= 
                            break;
                    }
                    mr = teknisyenHareket.Update();
                    result = WmrResult(teknisyenHareket, mr);
                }
            }
            return result;
        }


    }




    //public class ViewServisIsEmirler : SasonBase.Sason.Tables.Table_SERVISISEMIRLER.RawItem
    //{
    //    public decimal  ID { get; set; }
    //    protected decimal SERVISARACID { get; set; }
    //    public Decimal SERVISID { get; set; }
    //    public Decimal TEKNIKOLARAKTAMAMLA { get; set; }

    //    [DbTargetField("MUSTERIAD")] public string AraciGetiren { get; set; }
    //    [DbTargetField("ACIKLAMA")] public string Aciklama { get; set; }
    //    [DbTargetField("ISEMIRNO")] public string IsEmirNo { get; set; }

    //    [DbTargetField("SASENO")] public String AracSaseNo { get; set; }
    //    [DbTargetField("PLAKA")] public String AracPlaka { get; set; }
    //    [DbTargetField("KM")] public decimal AracKm { get; set; }
    //    [DbTargetField("MODELNO")] public string AracModelNo { get; set; }

    //    public DateTime KAYITTARIH { get; set; }

    //    public IsEmirDurumu Durumu { get; set; }

    //    //[ReadOnlyMappedRelation]
    //    //[RelationCondition("SERVISARACID", "ID")]
    //    //public RptServisAraclar ServisAracKaydi { get; set; }

    //    //[ReadOnlyRelation]
    //    //[RelationCondition("ID","SERVISISEMIRID")]

    //    //internal List<RptServisIsEmirIslemlerCheck> Islemler { get; set; }

    //    //[ReadOnlyRelation]
    //    //[RelationCondition("ID", "ISEMIRID")]
    //    //public ViewHareketUstBilgi UstBilgi { get; set; }

    //    //public decimal ToplamCalisilanZaman
    //    //{
    //    //    get
    //    //    {
    //    //        //return UstBilgi.TeknisyenHareketler.Sum(t=> t.)
    //    //        decimal ret = 0;
    //    //        if (UstBilgi != null && UstBilgi.TeknisyenHareketler.isNotEmpty())
    //    //        {
    //    //            //UstBilgi.TeknisyenHareketler?.forEach(t => {
    //    //            //    if (t.CIKISTARIHI == DateTime.MinValue)
    //    //            //        t.CIKISTARIHI = DateTime.Now;
    //    //            //});

    //    //            ret = UstBilgi.TeknisyenHareketler.Sum(t => t.CalismaDakika);
    //    //        }
    //    //        return ret;
    //    //    }
    //    //}
    //}

    //public class ViewServisIsEmirIslemler : SasonBase.Sason.Tables.Table_SERVISISEMIRISLEMLER.RawItem
    //{
    //    public decimal ID { get; set; }
    //    public int SERVISISEMIRID { get; set; }
    //    public string ACIKLAMA { get; set; }

    //    [ReadOnlyRelation]
    //    [RelationCondition("ID", "SERVISISEMIRISLEMID")]
    //    List<ViewServisIsEmirIslemIscilikler> Iscilikler { get; set; }

    //    public int ToplamIscilikSayisi
    //    {
    //        get { return Iscilikler.count(); }
    //    }
    //}

    //public class ViewServisIsEmirIslemIscilikler : SasonBase.Sason.Tables.Table_SERVISISMISLEMISCILIKLER.RawItem
    //{
    //    public virtual Decimal ID { get; set; }
    //    public virtual Decimal SERVISISEMIRISLEMID { get; set; }
    //    public virtual Decimal ISCILIKID { get; set; }
    //    public virtual Decimal SERVISISCILIKID { get; set; }
    //    public virtual Decimal SERVISPAKETID { get; set; }
    //    protected virtual String ACIKLAMA { get; set; }
    //    public virtual Decimal DISARIDAYAPTIRDI { get; set; }
    //    public virtual Decimal BAKIMISLEMYNEDENID { get; set; }

    //    [ReadOnlyMappedRelation]
    //    [RelationCondition("ISCILIKID","ID")]
    //    public RptIscilik01 Iscilik { get; set; }

    //    [ReadOnlyMappedRelation]
    //    [RelationCondition("SERVISISCILIKID", "ID")]
    //    public RptServisIscilik01 ServisIscilik { get; set; }

    //    //public string Iscilik_AW_Aciklama
    //    //{
    //    //    get { return Iscilik?.AWAciklama; }
    //    //}

    //    public string Aciklama
    //    {
    //        get
    //        {
    //            string retAciklama = this.ACIKLAMA;
    //            if (retAciklama.isNullOrWhiteSpace() && ServisIscilik.isNotNull())
    //                retAciklama = ServisIscilik.KOD;
    //            return retAciklama;
    //        }
    //    }

    //    public decimal Iscilik_Dakika_Suresi
    //    {
    //        get
    //        {
    //            decimal ret = 0;
    //            if (Iscilik.isNotNull())
    //                ret = Iscilik.DakikaDegeri;
    //            else if (ServisIscilik.isNotNull())
    //                ret = ServisIscilik.SUREDK;
                    
    //            //return Iscilik.isNull() ? 0 : Iscilik.DakikaDegeri;
    //            return ret;
    //        }
    //    }

    //    //[ReadOnlyRelation]
    //    //[RelationCondition("ID", "ISEMIRISLEMISCILIKID")]
    //    //public ViewHareketUstBilgi UstBilgi { get; set; }

    //    //public decimal ToplamCalisilanZaman
    //    //{
    //    //    get
    //    //    {
    //    //        //return UstBilgi.TeknisyenHareketler.Sum(t=> t.)
    //    //        decimal ret = 0;
    //    //        if (UstBilgi != null)
    //    //        {
    //    //            //UstBilgi.TeknisyenHareketler?.forEach(t => {
    //    //            //    if (t.CIKISTARIHI == DateTime.MinValue)
    //    //            //        t.CIKISTARIHI = DateTime.Now;
    //    //            //});

    //    //            //ret = UstBilgi.TeknisyenHareketler.Sum(t => t.CalismaDakika);
    //    //        }
    //    //        return ret;
    //    //    }
    //    //}
    //}

}