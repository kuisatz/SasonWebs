// Write your Javascript code.
(function (window) {
    window["initializeReportDesigner"] = function (designerModel) {
        var initDataSources = function (dataSources, dataSourcesData) {
            for (var i = 0; i < dataSources.length; i++) {
                var dataSource = dataSources[i];
                dataSource.data = dataSourcesData[i];
            }
        };
        var getRootKeyName = function (obj) {
            for (var prop in obj) {
                if (obj.hasOwnProperty(prop)) {
                    return prop;
                }
            }
            return null;
        }
        var initGlobalization = function (currentCulture) {
            $.when(
                $.get("/ReportDesigner/GetSupplementalJson/"),
                $.get("/ReportDesigner/GetCldrDataJson/")
            ).then(function () {
                // Normalize $.get results, we only need the JSON, not request statuses.
                return [].slice.apply(arguments, [0]).map(function (result) {
                    return result[0];
                });
            }).then(Globalize.load)
              .then(function () {
                  Globalize.locale(currentCulture);
            });
        }
        var dxDesignerElement = $('.dx-designer')[0];

        if (designerModel.dataSources && designerModel.dataSourcesData)
            initDataSources(designerModel.dataSources, designerModel.dataSourcesData)

        var reportModelRootName = getRootKeyName(designerModel);
        var data = {
            report: ko.observable(function () {
                var dataReportBase = {};
                dataReportBase[reportModelRootName] = {
                    "@ControlType": designerModel[reportModelRootName]["@ControlType"]
                };
                return dataReportBase;
            }()),
            reportUrl: ko.observable(designerModel.reportUrl),
            availableDataSources: designerModel.dataSources,
            dataSourceRefs: designerModel.dataSourceRefs,
            subreports: designerModel.subreports,
            infoDefaults: designerModel.infoDefaults,
            state: {
                reportExtensions: designerModel.reportExtensions
            },
            connectionStrings: designerModel.wizardConnections,
            disableCustomSql: designerModel.disableCustomSql,
            reportStorageWebIsRegister: designerModel.reportStorageWebIsRegister
        };
        var callbacks = {
            customizeActions: function (actions) { },
            customizeParameterEditors: function (parameter, editorInfo) { },
            exitDesigner: function () { }
        };
        initGlobalization(designerModel.currentCulture);
        var designerBindModel = DevExpress.Designer.Report.createReportDesigner(dxDesignerElement, data, callbacks, designerModel.localization, designerModel.knownEnums, designerModel.handlerUri, designerModel.viewerHandlerUri);
        data.report(designerModel.reportModel);
        designerBindModel.isLoading(false);
    };
})(this)
