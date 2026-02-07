window.saveAsFile = (fileName, byteBase64) => {
    const link = document.createElement('a');
    link.href = "data:application/octet-stream;base64," + byteBase64;
    link.download = fileName; // suggests the filename
    document.body.appendChild(link);

    // Force a click
    link.click();

    document.body.removeChild(link);
};

window.downloadFileFromResponse = async (url) => {
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error("Download failed: " + response.status);
    }

    // Read filename from Content-Disposition
    let fileName = "download.bin";
    const disposition = response.headers.get("Content-Disposition");
    if (disposition && disposition.indexOf("filename=") !== -1) {
        fileName = disposition.split("filename=")[1].trim().replace(/"/g, "");
    }

    // Get file data
    const blob = await response.blob();
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};