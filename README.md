# Campus Cafe Ordering System

A comprehensive web-based ordering system designed for campus cafeterias and food vendors, built with ASP.NET Core 8.0 and modern web technologies.

## 🚀 Features

### Multi-Role System
- **Customers**: Browse menu, place orders, track delivery, manage profile
- **Vendors**: Manage menu items, process orders, view sales reports
- **Administrators**: System management, user oversight, platform analytics

### Core Functionalities
- **Real-time Order Tracking**: Live updates using SignalR technology
- **Menu Management**: Dynamic menu with categories, pricing, and availability
- **Payment Integration**: Multiple payment methods with credit system
- **Catering Services**: Large event catering application system
- **Review & Feedback**: Customer rating and feedback system
- **Business Analytics**: Comprehensive reporting for vendors and admins

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Real-time Communication**: SignalR
- **Architecture**: MVC Pattern with Service Layer

### Frontend
- **UI Framework**: Razor Pages with Bootstrap 5
- **JavaScript**: jQuery for dynamic interactions
- **Styling**: Custom CSS with responsive design
- **Icons**: Font Awesome integration

### Development Tools
- **IDE**: Visual Studio 2022
- **Version Control**: Git
- **Database Management**: SQL Server Management Studio

## 📋 Prerequisites

Before running this application, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

## 🚀 Getting Started

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/CampusCafeOrderingSystem.git
cd CampusCafeOrderingSystem
```

### 2. Database Setup

1. Run database migrations:
```bash
dotnet ef database update
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Run the Application
```bash
dotnet run
```

The application will be available at `https://localhost:5117` .

## 📊 Database Schema

The system uses the following main entities:

- **Users**: Customer, Vendor, and Admin accounts
- **MenuItems**: Food items with categories and pricing
- **Orders**: Order management with status tracking
- **OrderItems**: Individual items within orders
- **CateringApplications**: Large event catering requests
- **Reviews**: Customer feedback and ratings
- **UserCredits**: Credit system for payments
- **Feedbacks**: System feedback and support tickets

## 🔧 Configuration


### Identity Configuration
The system uses ASP.NET Core Identity with the following password requirements:
- Minimum 6 characters
- Requires uppercase and lowercase letters
- Requires at least one digit
- Account lockout after 5 failed attempts

## 🎯 Usage

### For Customers
1. **Registration**: Create an account with email verification
2. **Browse Menu**: View available items by category
3. **Place Orders**: Add items to cart and checkout
4. **Track Orders**: Monitor order status in real-time
5. **Leave Reviews**: Rate and review completed orders

### For Vendors
1. **Menu Management**: Add, edit, and manage menu items
2. **Order Processing**: Accept and update order status
3. **Sales Analytics**: View sales reports and statistics
4. **Customer Interaction**: Respond to customer reviews

### For Administrators
1. **User Management**: Oversee all user accounts
2. **System Monitoring**: Monitor platform performance
3. **Catering Approval**: Review and approve catering applications
4. **Analytics Dashboard**: Access comprehensive system reports

## 🏗️ Project Structure

```
CampusCafeOrderingSystem/
├── Controllers/           # MVC Controllers
├── Models/               # Data models and ViewModels
├── Views/                # Razor views and layouts
├── Services/             # Business logic services
├── Data/                 # Database context and migrations
├── Hubs/                 # SignalR hubs for real-time features
├── wwwroot/              # Static files (CSS, JS, images)
├── Areas/                # Identity area for authentication
└── docs/                 # Project documentation
```

## 🔄 API Endpoints

### Order Management
- `GET /Order/MyOrders` - Get user's orders
- `POST /Order/Create` - Create new order
- `PUT /Order/UpdateStatus` - Update order status

### Menu Management
- `GET /Menu/Index` - Get menu items
- `POST /Menu/Create` - Add new menu item (Vendor only)
- `PUT /Menu/Edit` - Update menu item (Vendor only)

### User Management
- `GET /User/Profile` - Get user profile
- `POST /User/UpdateProfile` - Update user information



## 📈 Performance Features

- **Caching**: Memory caching for frequently accessed data
- **Lazy Loading**: Efficient data loading with Entity Framework
- **Real-time Updates**: SignalR for instant notifications
- **Responsive Design**: Mobile-friendly interface
- **Image Optimization**: Compressed image uploads

## 🔒 Security Features

- **Authentication**: Secure user authentication with Identity
- **Authorization**: Role-based access control
- **Data Protection**: Encrypted sensitive data
- **Input Validation**: Server-side validation for all inputs
- **CSRF Protection**: Cross-site request forgery prevention



## 👥 Team

- **Project Lead**: [Yidu Hou]
- **Team member**: [Xuran Li]
- **Team member**: [Yuze Liang]
- **Team member**: [Sen Niu]
- **Team member**: [Mingyi Li]

## 📞 Support

For support and questions:
- Email: 214006340@qq.com

## 🔮 Future Enhancements

- [ ] Mobile application (iOS/Android)
- [ ] AI-powered recommendation system
- [ ] Integration with external payment gateways
- [ ] Multi-language support
- [ ] Advanced analytics dashboard
- [ ] Inventory management system
- [ ] Loyalty program features


**Built with ❤️ for campus communities**