$(document).ready(function () {
    SetupCity();
});

$("#city").change(function () {
    SetupDistrict();
});

$("#district").change(function () {
    SetupWards();
});

function SetupCity() {
    const selectCity = $("#city");
    const selectedCity = selectCity.val();

    selectCity.empty()
        .append(`<option selected disabled>Select City...</option>`);

    $.ajax({
        type: "get",
        // Update the URL to the API that provides Sri Lankan cities
        url: "https://api-for-sri-lankan-cities.com/",
        success: function (data) {
            const cities = data.sort((a, b) => a.name.localeCompare(b.name));
            $.each(cities, function (index, value) {
                var cityName = value.name;
                if (cityName == selectedCity) {
                    selectCity.append(
                        `<option value="${cityName}" selected>${cityName}</option>`
                    );
                } else {
                    selectCity.append(
                        `<option value="${cityName}">${cityName}</option>`
                    );
                }
            });
            SetupDistrict();
        },
    });
}

function SetupDistrict() {
    let cityName = $("#city").val();
    const selectDistrict = $("#district");
    const selectedDistrict = selectDistrict.val();

    selectDistrict.empty()
        .append(`<option selected disabled>Select District...</option>`);

    if (cityName != null) {
        $.ajax({
            type: "get",
            // Update the URL to fetch districts based on the selected city
            url: `https://api-for-sri-lankan-districts.com/${cityName}`,
            success: function (data) {
                const districts = data.sort((a, b) => a.name.localeCompare(b.name));
                $.each(districts, function (index, value) {
                    if (value.name == selectedDistrict) {
                        selectDistrict.append(
                            `<option value="${value.name}" selected>${value.name}</option>`
                        );
                    }
                    else {
                        selectDistrict.append(
                            `<option value="${value.name}">${value.name}</option>`
                        );
                    }
                });
                SetupWards();
            },
        });
    } else {
        SetupWards();
    }
}

function SetupWards() {
    let districtName = $("#district").val();
    const selectWard = $("#ward");
    const selectedWard = selectWard.val();

    selectWard.empty()
        .append(`<option selected disabled>Select Ward...</option>`);

    if (districtName != null) {
        $.ajax({
            type: "get",
            // Update the URL to fetch wards based on the selected district
            url: `https://api-for-sri-lankan-wards.com/${districtName}`,
            success: function (data) {
                const wards = data.sort((a, b) => a.name.localeCompare(b.name));
                $.each(wards, function (index, value) {
                    if (value.name == selectedWard) {
                        selectWard.append(
                            `<option value="${value.name}" selected>${value.name}</option>`
                        );
                    }
                    else {
                        selectWard.append(
                            `<option value="${value.name}">${value.name}</option>`
                        );
                    }
                });
            },
        });
    }
}
