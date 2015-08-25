app.directive('ngStaticHeight', function () {
	return function ($scope, $elem, attr) {
		$elem.height($(window).height() - attr.ngStaticHeight);
	};
}).directive('convertToNumber', function () {
	return {
		require: 'ngModel',
		link: function (scope, element, attrs, ngModel) {
			ngModel.$parsers.push(function (val) {
				return parseInt(val, 10);
			});
			ngModel.$formatters.push(function (val) {
				return '' + val;
			});
		}
	};
}).directive('routeHref', ['$location', function ($location) {
	return function ($scope, $elem, attr) {
		$elem.click(function () {
			$location.path(attr.routeHref);
			$scope.$apply();
		});
	};
}]).directive('backButton', function () {
    return {
		restrict: 'A',

		link: function (scope, element, attrs) {
			element.bind('click', goBack);

			function goBack() {
				history.back();
				scope.$apply();
			}
		}
    }
}).directive('closeButton', function () {
    return {
		restrict: 'A',

		link: function (scope, element, attrs) {
			element.bind('click', close);

			function close() {
				WeixinJSBridge.call('closeWindow');
			}
		}
    }
}).directive('cartAddButton', function () {
	return {
		restrict: 'A',
		link: function (scope, element, attrs) {
			element.bind('click', function () {
				$('#cart-animate').css({
					'display': 'block',
					'position':'absolute'
				});
			});
		}
	}
});