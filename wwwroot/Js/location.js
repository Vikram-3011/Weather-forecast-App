function getUserLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject('Geolocation is not supported by your browser.');
        } else {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    resolve({
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude,
                        accuracy: position.coords.accuracy
                    });
                },
                (error) => {
                    reject(`Geolocation error: ${error.message}`);
                }
            );
        }
    });
}


let documentClickListener = null;

window.addDocumentClickListener = (dotNetHelper) => {
    if (documentClickListener) return; // Prevent duplicate listeners
    documentClickListener = (event) => {
        // Notify the Blazor component about the click event
        dotNetHelper.invokeMethodAsync('OnDocumentClick');
    };
    document.addEventListener('click', documentClickListener);
};

window.removeDocumentClickListener = () => {
    if (documentClickListener) {
        document.removeEventListener('click', documentClickListener);
        documentClickListener = null;
    }
};
