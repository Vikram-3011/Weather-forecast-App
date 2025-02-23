window.getFragmentToken = () => {
    const hash = window.location.hash.substring(1); // Remove #
    const params = new URLSearchParams(hash);
    return params.get("access_token"); // Get access_token from fragment
};
