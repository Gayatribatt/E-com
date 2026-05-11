// ShopWave - Site JS

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
            var bsAlert = bootstrap.Alert.getInstance(alert);
            if (bsAlert) bsAlert.close();
        });
    }, 5000);

    // Quantity input validation
    document.querySelectorAll('input[type="number"][name="quantity"]').forEach(function (input) {
        input.addEventListener('change', function () {
            if (this.value < 1) this.value = 1;
            if (this.max && parseInt(this.value) > parseInt(this.max)) this.value = this.max;
        });
    });
});
