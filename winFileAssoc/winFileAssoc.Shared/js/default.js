// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkID=392286
(function () {
    "use strict";

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {

        var activationKind = args.detail.kind;

        if (activationKind === activation.ActivationKind.launch) {
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize
                // your application here.
            } else {
                // TODO: This application has been reactivated from suspension.
                // Restore application state here.
            }
            args.setPromise(WinJS.UI.processAll());
        }

        if (activationKind === Windows.ApplicationModel.Activation.ActivationKind.file) {
            var file = args.detail.files[0];
            var copiedFile;
            Windows.Storage.CachedFileManager.deferUpdates(file);
            file.getBasicPropertiesAsync()
                .then(function (fileProperties) {
                    console.log('original file name', file.name);
                    console.log('original file content type', file.contentType);
                    console.log('original file size', fileProperties.size);
                    return WinJS.Promise.as();
                })
                .then(function () {
                    return file.copyAsync(
                        Windows.Storage.ApplicationData.current.localFolder,
                        'copy' + file.name,
                        Windows.Storage.NameCollisionOption.replaceExisting
                    );
                })
                .then(function (copiedFileRef) {
                    copiedFile = copiedFileRef;
                    return copiedFile.getBasicPropertiesAsync();
                })
                .done(function (copiedFileProperties) {
                    console.log('copied file name', copiedFile.name);
                    console.log('copied file content type', copiedFile.contentType);
                    console.log('copied file size', copiedFileProperties.size);
                    Windows.Storage.CachedFileManager.completeUpdatesAsync(file)

                }, function (e) {
                    console.error(e);
                });
        }
    };

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state
        // that needs to persist across suspensions here. You might use the
        // WinJS.Application.sessionState object, which is automatically
        // saved and restored across suspension. If you need to complete an
        // asynchronous operation before your application is suspended, call
        // args.setPromise().
    };

    app.start();
})();