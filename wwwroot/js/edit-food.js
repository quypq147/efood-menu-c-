const editForm = document.getElementById('edit-food-form');
const cancelEdit = document.getElementById('cancel-edit');

// Xử lý khi nhấn nút "Lưu"
editForm.addEventListener('submit', function (event) {
  event.preventDefault();

  // Lấy dữ liệu từ form
  const foodName = document.getElementById('food-name').value;
  const foodId = document.getElementById('food-id').value;
  const foodCategory = document.getElementById('food-category').value;
  const foodPrice = document.getElementById('food-price').value;
  const foodVat = document.getElementById('food-vat').value;
  const foodDescription = document.getElementById('food-description').value;
  const foodOnline = document.getElementById('food-online').checked;

  // Tải ảnh lên (nếu có)
  const foodImage = document.getElementById('food-image').files[0]
    ? URL.createObjectURL(document.getElementById('food-image').files[0])
    : 'images/default-food.jpg'; // Ảnh mặc định nếu không tải ảnh lên

  // Tạo đối tượng món ăn
  const newFood = {
    name: foodName,
    id: foodId,
    category: foodCategory,
    price: foodPrice,
    vat: foodVat,
    description: foodDescription,
    online: foodOnline,
    image: foodImage,
  };

  // Lưu món ăn vào LocalStorage
  const foodList = JSON.parse(localStorage.getItem('foodList')) || [];
  foodList.push(newFood);
  localStorage.setItem('foodList', JSON.stringify(foodList));

  alert('Món ăn đã được lưu!');
  editForm.reset();
});

// Xử lý khi nhấn nút "Hủy bỏ"
cancelEdit.addEventListener('click', function () {
  editForm.reset();
});