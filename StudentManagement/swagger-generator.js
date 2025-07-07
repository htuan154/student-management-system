// server/swagger-generator.js

const swaggerAutogen = require('swagger-autogen')({ openapi: '3.0.0' });

const doc = {
  info: {
    title: 'Bookflow API',
    version: '1.0.0',
    description: 'API Documentation for the Bookflow Hotel Booking System',
  },
  servers: [
    {
      url: 'http://localhost:8080',
      description: 'Development Server'
    }
  ],
  // Cấu trúc chuẩn cho OpenAPI 3.0
  components: {
    // Định nghĩa các cấu trúc dữ liệu (schemas)
    schemas: {
      UserObject: {
        type: 'object',
        properties: {
          userId: { type: 'string', format: 'uuid', example: '123e4567-e89b-12d3-a456-426614174000' },
          username: { type: 'string', example: 'johndoe' },
          email: { type: 'string', format: 'email', example: 'johndoe@example.com' },
          fullName: { type: 'string', example: 'John Doe' },
          roleId: { type: 'integer', example: 3 },
          createdAt: { type: 'string', format: 'date-time' }
        }
      },
      RegisterRequest: {
        type: 'object',
        required: ['username', 'email', 'password', 'fullName'],
        properties: {
          username: { type: 'string', example: 'johndoe' },
          email: { type: 'string', format: 'email', example: 'johndoe@example.com' },
          password: { type: 'string', format: 'password', example: 'password123' },
          fullName: { type: 'string', example: 'John Doe' }
        }
      },
      LoginRequest: {
        type: 'object',
        required: ['email', 'password'],
        properties: {
          email: { type: 'string', format: 'email', example: 'johndoe@example.com' },
          password: { type: 'string', format: 'password', example: 'password123' }
        }
      }
    },
    // Định nghĩa cách xác thực bằng token
    securitySchemes: {
      bearerAuth: {
        type: 'http',
        scheme: 'bearer',
        bearerFormat: 'JWT',
      }
    }
  }
};

const outputFile = './swagger-output.json';
// Trỏ đến file server chính (index.js) để swagger có thể quét toàn bộ route
const endpointsFiles = ['./index.js'];

// Chạy generator
swaggerAutogen(outputFile, endpointsFiles, doc);
