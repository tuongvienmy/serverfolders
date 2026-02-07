window.fileInterop = {
    triggerFileInput: function (id) {
        var input = document.getElementById(id);
        if (input) {
            input.click();
        }
    }
};
