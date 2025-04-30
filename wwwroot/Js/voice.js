window.voiceAssistant = {
    startListening: function (dotNetRef) {
        const recognition = new (window.SpeechRecognition || window.webkitSpeechRecognition)();
        recognition.lang = 'en-US';
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.onresult = function (event) {
            const transcript = event.results[0][0].transcript;
            dotNetRef.invokeMethodAsync('ReceiveVoiceInput', transcript);
        };

        recognition.onerror = function (event) {
            console.error('Speech recognition error:', event.error);
        };

        recognition.start();
    }
};
